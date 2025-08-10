#!/bin/bash
# high-performance PROXMOX VM provisioning script
# purpose: create or update a linux VM tuned for near bare-metal performance for API / application workloads.
# features:
#   - intelligent argument parsing (order-agnostic for storage, disk size, iso)
#   - automatic core and RAM allocation with host reservation
#   - environment variable overrides for deterministic resource control
#   - hugepages auto-detection (opt out with NO_HUGEPAGES=1)
#   - virtio + multiqueue nic (when supported) for improved network throughput
#   - storage media detection (ssd/hdd) adjusts disk cache, discard, iothread
#   - LVM-thin sizing normalisation and thin pool usage advisory
#   - safe re-create (destroys partial VM remnants before create)
#   - EFI, Q35, agent, NUMA, CPU host passthrough
#   - automatic cloud-init path when a cloud image (qcow2/img) is supplied (Ubuntu and similar cloud images) with fallback to ISO
#   - hardware RNG device attachment for better entropy (can disable with DISABLE_RNG=1)
# environment variable overrides (external interface kept for backward compatibility):
#   VM_CORES=<int>                preset number of virtual machine CPU cores (overrides heuristic)
#   HOST_RESERVED_CORES=<int>     host CPU cores to reserve (recalculates VM allocation)
#   HOST_RESERVED_RAM_MB=<int>    host memory (in megabytes) to reserve (virtual machine gets remainder)
#   VM_RAM_MB=<int>               exact virtual machine memory (in megabytes) (host reserve derived)
#   NO_HUGEPAGES=1                disable huge pages even if present on host
#   CLOUDINIT_DISABLE=1           force disable cloud-init even if a cloud image is provided
#   CLOUDINIT_USER=<name>         cloud-init user name (defaults to 'clouduser' if cloud-init path used and not set)
#   CLOUDINIT_SSH_KEY_FILE=<path> path to public SSH key to inject (e.g. /root/.ssh/id_rsa.pub)
#   CLOUDINIT_IPCONFIG0=<cfg>     PROXMOX ipconfig0 value (e.g. ip=192.168.1.50/24,gw=192.168.1.1) defaults to dhcp
#   DISABLE_RNG=1                 disable adding a host RNG device (virtio-rng) for entropy
#   FAIL_ON_WARN=1                convert certain warnings (e.g. missing ISO on update) into errors
#   MULTIQUEUE_MAX=<int>          cap for network multiqueue (default 8, PROXMOX allows higher on some versions; hard cap 16 here)
#   DISABLE_THIN_POOL_WARNING=1   suppress thin pool usage advisory
#   FORCE_SCSI_IO_THREAD=1        force iothread=1 even on HDD storage
#   VM_CPUSET=<list>              explicit CPU core list (e.g. 0-3,8) passed to PROXMOX --cpuset
#   DISABLE_DISCARD=1             remove discard=on from disk options (useful for certain storage backends)
#   DRY_RUN=1                     print qm commands without executing them
#   BALLOON_ENABLE=1              enable memory ballooning (omit --balloon 0)
#   NO_MEMORY_HOTPLUG=1           remove memory from hotplug features (avoids 1024MB alignment requirement)
# exit behavior: set -euo pipefail + ERR trap abort on first error.

# trap and print generic error then exit
trap 'echo "An Error Has Occurred"; exit 1' ERR

# enforce strict error handling (e=exit, u=unset vars, o=pipefail)
set -euo pipefail

# ====== DEFAULTS ======
# purpose: seed baseline storage and disk size so caller can omit optional args
# contents:
#   - DEFAULT_STORAGE: fallback proxmox storage name
#   - DEFAULT_DISK_SIZE: fallback virtual disk size (human readable)
# notes:
#   - overridden by positional args (storage / size)
#   - does not override explicit environment overrides
# extension: add future common defaults here (e.g. default bridge) maintaining single source

# default storage backend name
DEFAULT_STORAGE="local-lvm"

# default disk size for new VM creations
DEFAULT_DISK_SIZE="64G"

usage() { # print syntax help and examples then exit
    echo "Usage: $0 VIRTUAL_MACHINE_ID VIRTUAL_MACHINE_NAME [STORAGE_NAME] [DISK_SIZE_SPEC] [ISO_FILE_PATH]"
    echo "  VIRTUAL_MACHINE_ID    - Numeric Identifier Of The Virtual Machine To Create Or Update (Required)"
    echo "  VIRTUAL_MACHINE_NAME  - Human Friendly Name Of The Virtual Machine (Required)"
    echo "  STORAGE_NAME          - PROXMOX Storage Name (Optional, Default: $DEFAULT_STORAGE)"
    echo "  DISK_SIZE_SPEC        - Disk Size For New Virtual Machine (Optional, Default: $DEFAULT_DISK_SIZE)"
    echo "  ISO_FILE_PATH         - Full Path To ISO Image (Required For Create, Optional For Update)"
    echo ""
    echo "Examples:"
    echo "  $0 666 project-kongor /var/lib/vz/template/iso/server.iso"
    echo "  $0 666 project-kongor local-lvm /var/lib/vz/template/iso/server.iso"  
    echo "  $0 666 project-kongor 128G /var/lib/vz/template/iso/server.iso"
    echo "  $0 666 project-kongor local-lvm 128G /var/lib/vz/template/iso/server.iso"

    exit 1
}

# ensure minimum required arguments present (VMID NAME)
if [ "$#" -lt 2 ]; then
    usage
fi

# ====== CONFIG ======
# purpose: parse and classify remaining positional args after vmid + name
# behavior:
#   - order agnostic pattern matching (size token, existing file, storage via pvesm)
#   - cloud images (.qcow2 / .img / .cloudimg) switch INSTALLATION_MODE unless CLOUDINIT_DISABLE set
#   - ambiguous file extensions default to iso
# outputs:
#   - vm id/name, storage name, disk size spec, iso path (optional), cloud image path (optional), installation mode
# edge cases:
#   - numeric-like storage names avoided via strict size regex
#   - file readability validated later (pre-provision checks)
# errors: only arg count enforced here; semantic validation deferred

# capture required args
VIRTUAL_MACHINE_ID=$1
VIRTUAL_MACHINE_NAME=$2

# initialise ISO file path placeholder
ISO_FILE_PATH=""

# initialise cloud image file path placeholder
CLOUD_IMAGE_PATH=""

# initialise storage name with default
STORAGE_NAME=$DEFAULT_STORAGE

# initialise disk size specification with default
DISK_SIZE_SPEC=$DEFAULT_DISK_SIZE

# parse remaining arguments intelligently (order independent after VMID and NAME)
# remove already processed mandatory args from list
shift 2  # remove VMID and NAME from argument list

# iterate remaining args and categorize by pattern/content
for arg in "$@"; do
    if [[ "$arg" =~ ^[0-9]+[GMKTP]$ ]]; then
        # treat token as disk size specification
        DISK_SIZE_SPEC="$arg"
    elif [[ -f "$arg" ]]; then
        # differentiate ISO vs cloud image by extension
        if [[ "$arg" =~ \.(iso|ISO)$ ]]; then
            ISO_FILE_PATH="$arg"
        elif [[ "$arg" =~ \.(qcow2?|img|cloudimg)$ ]]; then
            CLOUD_IMAGE_PATH="$arg"
        else
            # if ambiguous default to ISO path
            ISO_FILE_PATH="$arg"
        fi
    elif pvesm status -storage "$arg" &>/dev/null; then
        STORAGE_NAME="$arg"
    elif [[ "$arg" =~ \.(iso|ISO)$ ]]; then
        ISO_FILE_PATH="$arg"
    elif [[ "$arg" =~ \.(qcow2?|img|cloudimg)$ ]]; then
        CLOUD_IMAGE_PATH="$arg"
    else
        STORAGE_NAME="$arg"
    fi
done

# decide installation mode
INSTALLATION_MODE="ISO"
if [ -n "$CLOUD_IMAGE_PATH" ] && [ -z "${CLOUDINIT_DISABLE:-}" ]; then
    INSTALLATION_MODE="CLOUD_IMAGE"
fi

# ====== PRE-PROVISION CHECKS ======
# purpose: validate identifiers, storage, and source media before resource math or create/update
# validates:
#   - vmid numeric & non-zero
#   - vm name set
#   - required commands present (qm, pvesm, awk, grep, sed)
#   - storage exists (pvesm status)
#   - iso or cloud image presence/readability per mode (warn vs fail on update via FAIL_ON_WARN)
# notes:
#   - emits single debug line summarizing parsed inputs
#   - no host state changes yet; safe to abort

# validate identifiers, storage existence, ISO accessibility, warn if ISO missing on updates
# emit debug summary of parsed inputs

# debug placeholder for unset values
UNSET_PLACEHOLDER="<UNSET>"
ISO_FILE_PATH_DISPLAY="$ISO_FILE_PATH"
if [ -z "$ISO_FILE_PATH_DISPLAY" ]; then ISO_FILE_PATH_DISPLAY="$UNSET_PLACEHOLDER"; fi
CLOUD_IMAGE_PATH_DISPLAY="$CLOUD_IMAGE_PATH"
if [ -z "$CLOUD_IMAGE_PATH_DISPLAY" ]; then CLOUD_IMAGE_PATH_DISPLAY="$UNSET_PLACEHOLDER"; fi
echo "DEBUG: VIRTUAL_MACHINE_ID=$VIRTUAL_MACHINE_ID, NAME=$VIRTUAL_MACHINE_NAME, STORAGE_NAME=$STORAGE_NAME, DISK_SIZE_SPEC=$DISK_SIZE_SPEC, ISO_FILE_PATH=$ISO_FILE_PATH_DISPLAY, CLOUD_IMAGE_PATH=$CLOUD_IMAGE_PATH_DISPLAY, INSTALLATION_MODE=$INSTALLATION_MODE"

# validate VMID is positive integer (no leading zero)
if ! [[ "$VIRTUAL_MACHINE_ID" =~ ^[1-9][0-9]*$ ]]; then
    echo "ERROR: Invalid VMID"; exit 1
fi

# minimal dependency check (core commands) - extendable
for REQUIRED_COMMAND in qm pvesm awk grep sed; do
    if ! command -v "$REQUIRED_COMMAND" >/dev/null 2>&1; then
        echo "ERROR: Required Command Not Found In PATH: $REQUIRED_COMMAND"; exit 1
    fi
done

# validate VM name non-empty and not placeholder
if [[ -z "$VIRTUAL_MACHINE_NAME" || "$VIRTUAL_MACHINE_NAME" == "..." ]]; then
    echo "ERROR: A Valid VM Name Must Be Set"; exit 1
fi

# verify storage backend exists on host
if ! pvesm status -storage "$STORAGE_NAME" &>/dev/null; then
    echo "ERROR: Storage '$STORAGE_NAME' Does Not Exist Or Is Not Available"
    echo "Available Storage:"
    pvesm status | awk 'NR>1 {print "  " $1}'
    exit 1
fi

# enforce source presence for create operations only (ISO or cloud image)
if [[ "$INSTALLATION_MODE" == "ISO" ]]; then
    if [[ ! -f "$ISO_FILE_PATH" ]]; then
        if qm status "$VIRTUAL_MACHINE_ID" &>/dev/null; then
            echo "WARNING: ISO File Not Found: $ISO_FILE_PATH (OK For Virtual Machine Updates)"
            if [ -n "${FAIL_ON_WARN:-}" ]; then echo "ERROR: FAIL_ON_WARN Set - Aborting"; exit 1; fi
            ISO_FILE_PATH=""
        else
            echo "ERROR: ISO File Required For New Virtual Machine Creation (or provide a cloud image)"; exit 1
        fi
    fi
else
    if [[ ! -f "$CLOUD_IMAGE_PATH" ]]; then
    echo "ERROR: Cloud Image Path Is Set But File Is Missing: $CLOUD_IMAGE_PATH"; exit 1
    fi
fi

# ensure source readable if path supplied
if [[ "$INSTALLATION_MODE" == "ISO" ]]; then
    if [[ -n "$ISO_FILE_PATH" && ! -r "$ISO_FILE_PATH" ]]; then
    echo "ERROR: ISO File Exists But Is Not Readable: $ISO_FILE_PATH"; exit 1
    fi
else
    if [[ -n "$CLOUD_IMAGE_PATH" && ! -r "$CLOUD_IMAGE_PATH" ]]; then
    echo "ERROR: Cloud Image File Exists But Is Not Readable: $CLOUD_IMAGE_PATH"; exit 1
    fi
fi

# ====== CALCULATE HOST SPECIFICATIONS ======
# purpose: compute vm cpu + memory allocations with host reservation heuristics
# steps:
#   - read total cores & mem
#   - apply default reservation (1 core, 2g or 25% on small hosts)
#   - apply overrides (VM_CORES / HOST_RESERVED_CORES / VM_RAM_MB / HOST_RESERVED_RAM_MB)
#   - derive allocated vs reserved metrics
# huge pages:
#   - auto pick 1g or 2m; disable via NO_HUGEPAGES
# memory hot-plug:
#   - align memory to 1024MB boundary when enabled; recompute host reserve
# ballooning:
#   - fixed by default (balloon 0) unless BALLOON_ENABLE set
# cpu pinning:
#   - optional VM_CPUSET -> --cpuset
# safety:
#   - enforce min 1 core, min host reserve, clamp oversized vm memory

# collect host CPU core and memory information and derive virtual machine allocations
# CPU core strategy: reserve 1 CPU core by default (unless only one is available) unless overridden
# memory strategy: reserve 2 GB (or 25% on very small hosts) unless overridden
# deterministic sizing overrides: VM_RAM_MB or HOST_RESERVED_RAM_MB (environment variables) allow explicit sizing

TOTAL_HOST_CPU_CORES=$(nproc --all) # total logical host CPU cores (including hyperthreads)

# Map external environment variable VM_CORES into internal descriptive name ALLOCATED_VM_CPU_CORES if provided
if [ -n "${VM_CORES:-}" ]; then
    ALLOCATED_VM_CPU_CORES=$VM_CORES
else
    ALLOCATED_VM_CPU_CORES=$TOTAL_HOST_CPU_CORES
    if [ "$ALLOCATED_VM_CPU_CORES" -gt 1 ]; then ALLOCATED_VM_CPU_CORES=$((ALLOCATED_VM_CPU_CORES-1)); fi
fi

# HOST_RESERVED_CORES override: invert to recalculate allocation enforcing a minimum of 1 CPU core
if [ -n "${HOST_RESERVED_CORES:-}" ]; then
    ALLOCATED_VM_CPU_CORES=$(( TOTAL_HOST_CPU_CORES - HOST_RESERVED_CORES ))
    if [ "$ALLOCATED_VM_CPU_CORES" -lt 1 ]; then ALLOCATED_VM_CPU_CORES=1; fi
    RESERVED_HOST_CPU_CORES=$HOST_RESERVED_CORES
else
    RESERVED_HOST_CPU_CORES=$(( TOTAL_HOST_CPU_CORES - ALLOCATED_VM_CPU_CORES ))
fi

# reserve baseline memory (2 GB or 25% if system is small); remainder to virtual machine
# overrides:
#   - HOST_RESERVED_RAM_MB fixes host memory reserve (virtual machine gets remainder)
#   - VM_RAM_MB fixes virtual machine memory (host reserve derived) and wins if both are set
TOTAL_HOST_MEMORY_MB=$(awk '/MemTotal/ {print int($2/1024)}' /proc/meminfo)
RESERVED_HOST_MEMORY_MB=${HOST_RESERVED_RAM_MB:-2048}
if [ "$TOTAL_HOST_MEMORY_MB" -lt 8192 ]; then # on small hosts keep at least 25% for host
    HOST_RESERVE_CANDIDATE_MB=$(( TOTAL_HOST_MEMORY_MB / 4 ))
    if [ "$HOST_RESERVE_CANDIDATE_MB" -gt "$RESERVED_HOST_MEMORY_MB" ]; then RESERVED_HOST_MEMORY_MB=$HOST_RESERVE_CANDIDATE_MB; fi
fi
if [ "$RESERVED_HOST_MEMORY_MB" -ge "$TOTAL_HOST_MEMORY_MB" ]; then RESERVED_HOST_MEMORY_MB=$(( TOTAL_HOST_MEMORY_MB / 2 )); fi

# explicit VM_RAM_MB override takes precedence over host reserve variable
if [ -n "${VM_RAM_MB:-}" ]; then
    if [ "$VM_RAM_MB" -ge "$TOTAL_HOST_MEMORY_MB" ]; then VM_RAM_MB=$(( TOTAL_HOST_MEMORY_MB - 512 )); fi
    RESERVED_HOST_MEMORY_MB=$(( TOTAL_HOST_MEMORY_MB - VM_RAM_MB ))
    ALLOCATED_VM_MEMORY_MB=$VM_RAM_MB
else
    ALLOCATED_VM_MEMORY_MB=$(( TOTAL_HOST_MEMORY_MB - RESERVED_HOST_MEMORY_MB ))
fi

# huge pages optimisation if configured on host (transparent huge pages not enforced here)
HUGE_PAGES_OPTION=""
if grep -q 'HugePages_Total:[[:space:]]*[1-9]' /proc/meminfo 2>/dev/null; then
    # detect huge page size (kB) to choose 1G pages if dominant, else 2M
    HUGE_PAGE_SIZE_KB=$(awk '/Hugepagesize/ {print $2}' /proc/meminfo 2>/dev/null || echo 2048)
    if [ "$HUGE_PAGE_SIZE_KB" -ge 1048576 ]; then
        HUGE_PAGES_OPTION="--hugepages 1024" # request 1G huge pages when configured
    else
        HUGE_PAGES_OPTION="--hugepages 2"     # default to 2M huge pages
    fi
fi

# allow disabling huge pages via NO_HUGEPAGES=1 (useful for debugging or mixed workloads)
if [ -n "${NO_HUGEPAGES:-}" ]; then
    HUGE_PAGES_OPTION=""
fi

# optional cpuset for explicit core pinning
CPUSET_OPTION=""
if [ -n "${VM_CPUSET:-}" ]; then
    CPUSET_OPTION="--cpuset ${VM_CPUSET}"
fi

# balloon parameter toggle (default disabled / fixed memory)
if [ -n "${BALLOON_ENABLE:-}" ]; then
    BALLOON_PARAMETER="" # let PROXMOX default (enables ballooning)
else
    BALLOON_PARAMETER="--balloon 0"
fi

# memory hotplug requires memory size aligned to 1024MB (1GiB) boundaries in current PROXMOX versions
# introduce optional opt-out of memory hotplug via NO_MEMORY_HOTPLUG=1
HOTPLUG_FEATURES="disk,network,memory,cpu"
if [ -n "${NO_MEMORY_HOTPLUG:-}" ]; then
    HOTPLUG_FEATURES="disk,network,cpu"
fi

# if memory hotplug remains enabled and computed memory isn't 1024-aligned, floor-align it to satisfy qm validation
if [[ "$HOTPLUG_FEATURES" == *memory* ]]; then
    if (( ALLOCATED_VM_MEMORY_MB % 1024 != 0 )); then
        ORIGINAL_ALLOCATED_VM_MEMORY_MB=$ALLOCATED_VM_MEMORY_MB
        ALLOCATED_VM_MEMORY_MB=$(( (ALLOCATED_VM_MEMORY_MB / 1024) * 1024 ))
        # ensure a minimum of 1024MB after alignment (edge case very small hosts)
        if (( ALLOCATED_VM_MEMORY_MB < 1024 )); then ALLOCATED_VM_MEMORY_MB=1024; fi
    # recompute host reserved memory to maintain total consistency
    RESERVED_HOST_MEMORY_MB=$(( TOTAL_HOST_MEMORY_MB - ALLOCATED_VM_MEMORY_MB ))
    if (( RESERVED_HOST_MEMORY_MB < 512 )); then RESERVED_HOST_MEMORY_MB=512; fi
    echo "INFO: Adjusted Memory From ${ORIGINAL_ALLOCATED_VM_MEMORY_MB}MB To ${ALLOCATED_VM_MEMORY_MB}MB (Aligned To 1024MB For Memory Hot-Plug)"
    fi
fi

# virtio multiqueue for network (maximum 8) if supported; improves parallel packet processing
NETWORK_DEVICE0_MULTIQUEUE_COUNT=$ALLOCATED_VM_CPU_CORES
MULTIQUEUE_HARD_CAP=8
if [ -n "${MULTIQUEUE_MAX:-}" ]; then
    if [[ "$MULTIQUEUE_MAX" =~ ^[0-9]+$ ]]; then
        # allow up to 16 as a safety upper bound even if user sets higher
        if [ "$MULTIQUEUE_MAX" -gt 16 ]; then MULTIQUEUE_MAX=16; fi
        MULTIQUEUE_HARD_CAP=$MULTIQUEUE_MAX
    fi
fi
if [ "$NETWORK_DEVICE0_MULTIQUEUE_COUNT" -gt "$MULTIQUEUE_HARD_CAP" ]; then NETWORK_DEVICE0_MULTIQUEUE_COUNT=$MULTIQUEUE_HARD_CAP; fi
NETWORK_DEVICE0_BASE="virtio,bridge=vmbr0,firewall=1"
if qm help create 2>&1 | grep -qi 'multiqueue'; then
    NETWORK_DEVICE0_PARAMETER="${NETWORK_DEVICE0_BASE},multiqueue=${NETWORK_DEVICE0_MULTIQUEUE_COUNT}"
else
    NETWORK_DEVICE0_PARAMETER="$NETWORK_DEVICE0_BASE"
fi

# ====== DETECT IF STORAGE IS SSD ======
# purpose: pick disk option profile (ssd vs hdd) for optimal cache + iothread + discard settings
# method:
#   - inspect storage type then underlying block device rotational flag
# profiles:
#   - ssd: ssd=1, discard=on, cache=writeback, iothread=1
#   - hdd: discard=on, cache=writeback (iothread optional via FORCE_SCSI_IO_THREAD)
# modifiers:
#   - DISABLE_DISCARD strips discard=on
#   - DISABLE_THIN_POOL_WARNING suppresses lvmthin usage warning
#   - FORCE_SCSI_IO_THREAD adds iothread=1 on rotational media
# advisory:
#   - warn at >=85% thin pool data usage to preempt ENOSPC

# determine rotational vs SSD to choose disk options:
#   SSD: add ssd=1, discard=on, cache=writeback, iothread=1
#   HDD: discard=on, cache=writeback (omit iothread)
echo "Detecting Storage Type For $STORAGE_NAME..."

# get storage configuration from PROXMOX
STORAGE_STATUS_LINE=$(pvesm status -storage "$STORAGE_NAME" -content images 2>/dev/null | tail -n1)
STORAGE_TYPE_KIND=$(echo "$STORAGE_STATUS_LINE" | awk '{print $2}')

# try to detect SSD based on storage type and underlying device
ROTATIONAL=1 # default to HDD

case "$STORAGE_TYPE_KIND" in
    "zfspool"|"lvm"|"lvmthin")
        # for ZFS/LVM storages, try to find the underlying device
    if command -v zfs >/dev/null 2>&1 && [[ "$STORAGE_TYPE_KIND" == "zfspool" ]]; then
            # ZFS pool - check underlying devices
            POOL_DEVICES=$(zpool list -v "$STORAGE_NAME" 2>/dev/null | awk '/\/dev\// {print $1}' | head -1)
            if [[ -n "$POOL_DEVICES" ]]; then
                DEVICE=$(basename "$POOL_DEVICES")
                if [[ -f "/sys/block/$DEVICE/queue/rotational" ]]; then
                    ROTATIONAL=$(cat "/sys/block/$DEVICE/queue/rotational" 2>/dev/null || echo 1)
                fi
            fi
        elif [[ "$STORAGE_TYPE_KIND" =~ ^lvm ]]; then
            # LVM - find the physical volume
            VG_NAME=$(pvesm path "$STORAGE_NAME:" 2>/dev/null | grep -o 'dev/[^/]*' | cut -d'/' -f2 2>/dev/null || echo "")
            if [[ -n "$VG_NAME" ]]; then
                PV_DEVICE=$(pvs --noheadings -o pv_name -S vg_name="$VG_NAME" 2>/dev/null | tr -d ' ' | head -1)
                if [[ -n "$PV_DEVICE" ]]; then
                    DEVICE=$(basename "$PV_DEVICE" | sed 's/[0-9]*$//')
                    if [[ -f "/sys/block/$DEVICE/queue/rotational" ]]; then
                        ROTATIONAL=$(cat "/sys/block/$DEVICE/queue/rotational" 2>/dev/null || echo 1)
                    fi
                fi
            fi
        fi
        ;;
    "dir")
    # directory storage - check the filesystem's underlying device
    STORAGE_PATH=$(pvesm path "$STORAGE_NAME:" 2>/dev/null | head -1)
        if [[ -d "$(dirname "$STORAGE_PATH")" ]]; then
            DEVICE=$(df "$(dirname "$STORAGE_PATH")" --output=source 2>/dev/null | tail -n1 | sed 's/[^a-zA-Z0-9]*//g')
            if [[ -f "/sys/block/$DEVICE/queue/rotational" ]]; then
                ROTATIONAL=$(cat "/sys/block/$DEVICE/queue/rotational" 2>/dev/null || echo 1)
            fi
        fi
        ;;
esac

if [ "$ROTATIONAL" -eq 0 ]; then
    echo "Detected SSD Storage"
    PRIMARY_DISK_OPTIONS="ssd=1,discard=on,cache=writeback,iothread=1"
else
    echo "Detected HDD Storage (Or Unable To Determine)"
    if [ -n "${FORCE_SCSI_IO_THREAD:-}" ]; then
        PRIMARY_DISK_OPTIONS="discard=on,cache=writeback,iothread=1"
    else
        PRIMARY_DISK_OPTIONS="discard=on,cache=writeback"
    fi
fi

# optionally remove discard directive if disabled
if [ -n "${DISABLE_DISCARD:-}" ]; then
    PRIMARY_DISK_OPTIONS="${PRIMARY_DISK_OPTIONS//,discard=on/}"
    PRIMARY_DISK_OPTIONS="${PRIMARY_DISK_OPTIONS//discard=on,}"
    PRIMARY_DISK_OPTIONS="${PRIMARY_DISK_OPTIONS//discard=on/}"
fi

# helper: normalize a human disk size (e.g. 128G, 64, 1T, 512M) to whole GB integer
# required for LVM-thin shorthand (storage:SIZE) which accepts integer GB only
normalize_to_gb() {
    local INPUT_UPPER="${1^^}" # uppercase units
    if [[ "$INPUT_UPPER" =~ ^([0-9]+)([KMGTP])?$ ]]; then
        local NUM="${BASH_REMATCH[1]}"
        local UNIT="${BASH_REMATCH[2]}"
        case "$UNIT" in
            P) echo $(( NUM * 1024 * 1024 )) ;; # petabytes -> GB (unlikely)
            T) echo $(( NUM * 1024 )) ;;
            G|"") echo $NUM ;;
            M) # round up partial GB
               echo $(( (NUM + 1023) / 1024 )) ;;
            K) echo 1 ;;
            *) echo "$NUM" ;;
        esac
    else
        # fallback: strip trailing non-digits then assume GB
        echo "${1%%[^0-9]*}"
    fi
}

# helper: convert human size spec (e.g. 10G, 512M) to bytes (integer) for comparisons
size_spec_to_bytes() {
    local INPUT_UPPER="${1^^}";
    if [[ "$INPUT_UPPER" =~ ^([0-9]+)([KMGTP])?$ ]]; then
        local NUM="${BASH_REMATCH[1]}"; local UNIT="${BASH_REMATCH[2]}";
        case "$UNIT" in
            P) echo $(( NUM * 1024 * 1024 * 1024 * 1024 * 1024 )) ;; # PB to bytes
            T) echo $(( NUM * 1024 * 1024 * 1024 * 1024 * 1024 )) ;;
            G|"") echo $(( NUM * 1024 * 1024 * 1024 )) ;;
            M) echo $(( NUM * 1024 * 1024 )) ;;
            K) echo $(( NUM * 1024 )) ;;
            *) echo $NUM ;;
        esac
    else
        echo "${1%%[^0-9]*}"
    fi
}

# format disk specification based on storage type (LVM-thin path adds DISK_OPTS for discard)
case "$STORAGE_TYPE_KIND" in
    "lvmthin")
        # for lvm-thin: PROXMOX expects storage:SIZE (SIZE = integer GB, no unit suffix)
    SIZE_GB=$(normalize_to_gb "$DISK_SIZE_SPEC")
        if [[ -z "$SIZE_GB" || "$SIZE_GB" -le 0 ]]; then
            echo "ERROR: Unable To Derive Valid GB Size From '$DISK_SIZE_SPEC'" >&2
            exit 1
        fi
    PRIMARY_DISK_SCSI0_PARAMETER="${STORAGE_NAME}:${SIZE_GB},${PRIMARY_DISK_OPTIONS}"

        # EFI disk: just allocate 1GB (minimum granularity in shorthand syntax)
    EFI_DISK_PARAMETER="${STORAGE_NAME}:1,efitype=4m,pre-enrolled-keys=0"

    # thin pool capacity advisory (best-effort, non-fatal)
    # uses lvs data_percent to warn at >=85% utilisation to avoid ENOSPC events
    if command -v lvs >/dev/null 2>&1 && [ -z "${DISABLE_THIN_POOL_WARNING:-}" ]; then
            THIN_DATA_PCT=$(lvs --noheadings -o data_percent 2>/dev/null | awk 'NR==1 {gsub(/\.[0-9]+/,"",$1); print $1}')
            if [[ -n "$THIN_DATA_PCT" ]]; then
                if [ "$THIN_DATA_PCT" -ge 85 ]; then
                    echo "WARNING: Thin Pool Usage At ${THIN_DATA_PCT}% (>=85%). Consider Extending Before Heavy Writes." >&2
                fi
            fi
        fi
        ;;
    *)
        # other storage types can keep human-readable size with options
    PRIMARY_DISK_SCSI0_PARAMETER="${STORAGE_NAME}:${DISK_SIZE_SPEC},${PRIMARY_DISK_OPTIONS}"
    EFI_DISK_PARAMETER="${STORAGE_NAME}:1,efitype=4m,pre-enrolled-keys=0"
        ;;
esac

# dry run handling for qm command execution
QM_EXEC_PREFIX=""
if [ -n "${DRY_RUN:-}" ]; then
    echo "INFO: DRY_RUN Enabled - Commands Will Be Echoed Only"
    QM_EXEC_PREFIX="echo DRY_RUN:"
fi

# ====== CREATE OR UPDATE VM ======
# purpose: idempotent provisioning (create new or tune existing vm)
# update path:
#   - adjust cores / memory / cpu model / numa / balloon / cpuset only
# create path:
#   - destroy partial remnants then qm create with: uefi (ovmf), q35, virtio-scsi-single + iothread, agent, hot-plug list, huge pages (optional), rng (optional)
#   - cloud-init mode: import disk, attach cloudinit drive, set user, ssh key, ipconfig, optional resize
#   - iso mode: attach cdrom
# controls:
#   - DRY_RUN echoes commands
#   - NO_MEMORY_HOTPLUG trims memory from hot-plug list
# resilience:
#   - non-critical steps tolerate failure (resize, rng) using || true
# guidance:
#   - final echo lines show start, terminal, config, guest agent trim command

# update mode: only CPU/RAM adjusted (idempotent tuning)
# create mode: full VM definition with performance flags:
#   - Q35 + OVMF (UEFI)
#   - virtio-scsi-single with iothread
#   - agent enabled (integration + fstrim on clones)
#   - ostype l26 (modern Linux optimizations)
#   - NUMA 1 (enable NUMA awareness)
#   - hotplug enabled (disk, network, memory, CPU)
VM_EXISTS=0
if [ -z "${DRY_RUN:-}" ] && qm status "$VIRTUAL_MACHINE_ID" &>/dev/null; then
    VM_EXISTS=1
fi
if [ "$VM_EXISTS" -eq 1 ]; then
    echo "Virtual Machine $VIRTUAL_MACHINE_ID Exists - Updating CPU Cores And Memory To Maximum Safe Values..."
    $QM_EXEC_PREFIX qm set "$VIRTUAL_MACHINE_ID" \
        --cores "$ALLOCATED_VM_CPU_CORES" --sockets 1 --cpu host --numa 1 $CPUSET_OPTION \
        --memory "$ALLOCATED_VM_MEMORY_MB" $BALLOON_PARAMETER
else
    echo "Virtual Machine $VIRTUAL_MACHINE_ID Does Not Exist - Creating High-Performance Virtual Machine..."
    echo "  CPU Cores (Host/Allocated/Reserved): ${TOTAL_HOST_CPU_CORES}/${ALLOCATED_VM_CPU_CORES}/${RESERVED_HOST_CPU_CORES}"
    echo "  Memory (Host/Allocated/Reserved): ${TOTAL_HOST_MEMORY_MB}MB/${ALLOCATED_VM_MEMORY_MB}MB/${RESERVED_HOST_MEMORY_MB}MB"
    echo "  Storage: $STORAGE_NAME ($STORAGE_TYPE_KIND)"
    echo "  Disk: ${DISK_SIZE_SPEC} With Options: $PRIMARY_DISK_OPTIONS"
    if [ "$INSTALLATION_MODE" = "CLOUD_IMAGE" ]; then
    echo "  Installation Mode: CLOUD-INIT (Cloud Image: $CLOUD_IMAGE_PATH)"
    else
        echo "  Installation Mode: ISO"
    fi

    # clean up any partial VM creation artifacts (ignores error if absent)
    $QM_EXEC_PREFIX qm destroy "$VIRTUAL_MACHINE_ID" 2>/dev/null || true

    if [ "$INSTALLATION_MODE" = "CLOUD_IMAGE" ]; then
        # base create without primary disk (will import cloud image)
        $QM_EXEC_PREFIX qm create "$VIRTUAL_MACHINE_ID" \
            --name "$VIRTUAL_MACHINE_NAME" \
            --machine q35 \
            --bios ovmf \
            --efidisk0 "$EFI_DISK_PARAMETER" \
            --cores "$ALLOCATED_VM_CPU_CORES" --sockets 1 --cpu host --numa 1 $CPUSET_OPTION \
            --memory "$ALLOCATED_VM_MEMORY_MB" $BALLOON_PARAMETER \
            --hotplug ${HOTPLUG_FEATURES} \
            --serial0 socket --vga serial0 \
            --agent enabled=1,fstrim_cloned_disks=1 \
            --ostype l26 \
            ${HUGE_PAGES_OPTION} \
            --onboot 0 \
            --protection 0

        # add RNG device unless disabled
        if [ -z "${DISABLE_RNG:-}" ]; then
            $QM_EXEC_PREFIX qm set "$VIRTUAL_MACHINE_ID" --rng0 source=/dev/urandom >/dev/null 2>&1 || true
        fi

    echo "Importing Cloud Image..."
    $QM_EXEC_PREFIX qm importdisk "$VIRTUAL_MACHINE_ID" "$CLOUD_IMAGE_PATH" "$STORAGE_NAME" >/dev/null 2>&1 || $QM_EXEC_PREFIX qm importdisk "$VIRTUAL_MACHINE_ID" "$CLOUD_IMAGE_PATH" "$STORAGE_NAME"

        # attach imported disk (assumes disk-0 naming convention)
    $QM_EXEC_PREFIX qm set "$VIRTUAL_MACHINE_ID" --scsihw virtio-scsi-single --scsi0 "${STORAGE_NAME}:vm-${VIRTUAL_MACHINE_ID}-disk-0,${PRIMARY_DISK_OPTIONS}" >/dev/null
        # add network
    $QM_EXEC_PREFIX qm set "$VIRTUAL_MACHINE_ID" --net0 "$NETWORK_DEVICE0_PARAMETER" >/dev/null
        # add cloud-init drive
    $QM_EXEC_PREFIX qm set "$VIRTUAL_MACHINE_ID" --ide2 "${STORAGE_NAME}:cloudinit" >/dev/null
        # configure boot order
    $QM_EXEC_PREFIX qm set "$VIRTUAL_MACHINE_ID" --boot order=scsi0 >/dev/null

        # cloud-init user
        CLOUDINIT_EFFECTIVE_USER=${CLOUDINIT_USER:-clouduser}
    $QM_EXEC_PREFIX qm set "$VIRTUAL_MACHINE_ID" --ciuser "$CLOUDINIT_EFFECTIVE_USER" >/dev/null
        # ssh key injection if provided
        if [ -n "${CLOUDINIT_SSH_KEY_FILE:-}" ] && [ -f "${CLOUDINIT_SSH_KEY_FILE}" ]; then
            $QM_EXEC_PREFIX qm set "$VIRTUAL_MACHINE_ID" --sshkey "${CLOUDINIT_SSH_KEY_FILE}" >/dev/null
        fi
        # network config (defaults to dhcp)
        CLOUDINIT_IPCONFIG0_VALUE=${CLOUDINIT_IPCONFIG0:-ip=dhcp}
    $QM_EXEC_PREFIX qm set "$VIRTUAL_MACHINE_ID" --ipconfig0 "$CLOUDINIT_IPCONFIG0_VALUE" >/dev/null

        # optional resize if user requested a larger disk size
        if [ -n "${DISK_SIZE_SPEC:-}" ]; then
            # only resize if requested size larger than current virtual size
            if command -v qemu-img >/dev/null 2>&1; then
                IMPORTED_DISK_PATH=$(pvesm path "${STORAGE_NAME}:vm-${VIRTUAL_MACHINE_ID}-disk-0" 2>/dev/null || echo "")
                if [ -n "$IMPORTED_DISK_PATH" ]; then
                    CURRENT_BYTES=$(qemu-img info --output=json "$IMPORTED_DISK_PATH" 2>/dev/null | awk -F'[,:]+' '/"virtual-size"/ {gsub(/[^0-9]/,"",$2); print $2; exit}')
                    REQUESTED_BYTES=$(size_spec_to_bytes "$DISK_SIZE_SPEC")
                    if [ -n "$CURRENT_BYTES" ] && [ -n "$REQUESTED_BYTES" ] && [ "$REQUESTED_BYTES" -gt "$CURRENT_BYTES" ]; then
                        $QM_EXEC_PREFIX qm resize "$VIRTUAL_MACHINE_ID" scsi0 "$DISK_SIZE_SPEC" >/dev/null 2>&1 || true
                    fi
                fi
            else
                $QM_EXEC_PREFIX qm resize "$VIRTUAL_MACHINE_ID" scsi0 "$DISK_SIZE_SPEC" >/dev/null 2>&1 || true
            fi
        fi
    else
        $QM_EXEC_PREFIX qm create "$VIRTUAL_MACHINE_ID" \
            --name "$VIRTUAL_MACHINE_NAME" \
            --machine q35 \
            --bios ovmf \
            --efidisk0 "$EFI_DISK_PARAMETER" \
            --cores "$ALLOCATED_VM_CPU_CORES" --sockets 1 --cpu host --numa 1 $CPUSET_OPTION \
            --memory "$ALLOCATED_VM_MEMORY_MB" $BALLOON_PARAMETER \
            --scsihw virtio-scsi-single \
            --scsi0 "$PRIMARY_DISK_SCSI0_PARAMETER" \
            --net0 "$NETWORK_DEVICE0_PARAMETER" \
            --hotplug ${HOTPLUG_FEATURES} \
            --boot order=scsi0 \
            --serial0 socket --vga serial0 \
            ${ISO_FILE_PATH:+--cdrom "$ISO_FILE_PATH"} \
            --agent enabled=1,fstrim_cloned_disks=1 \
            --ostype l26 \
            ${HUGE_PAGES_OPTION} \
            --onboot 0 \
            --protection 0
        # add RNG device unless disabled
        if [ -z "${DISABLE_RNG:-}" ]; then
            $QM_EXEC_PREFIX qm set "$VIRTUAL_MACHINE_ID" --rng0 source=/dev/urandom >/dev/null 2>&1 || true
        fi
    fi

    echo "Virtual Machine $VIRTUAL_MACHINE_ID Created Successfully"
fi

echo "DONE"
echo "Start With: qm start $VIRTUAL_MACHINE_ID"
echo "Connect In Terminal: qm terminal $VIRTUAL_MACHINE_ID"
echo "Inspect Config: qm config $VIRTUAL_MACHINE_ID"
echo "Discard Guest Agent (After Operating System Install): qm agent $VIRTUAL_MACHINE_ID fstrim" 2>/dev/null || true

# (optionally) destroy already created VM
# qm destroy 666 2>/dev/null || true

# (optionally) clear script (discouraged unless embedding elsewhere)
# > /root/provision-high-performance-server.sh

# create or update script
# nano /root/provision-high-performance-server.sh

# make it executable
# chmod +x /root/provision-high-performance-server.sh

# execute (examples)
# ./provision-high-performance-server.sh 666 project-kongor /var/lib/vz/template/iso/server.iso
# ./provision-high-performance-server.sh 666 project-kongor local-lvm /var/lib/vz/template/iso/server.iso
# ./provision-high-performance-server.sh 666 project-kongor 128G /var/lib/vz/template/iso/server.iso
# ./provision-high-performance-server.sh 666 project-kongor local-lvm 128G /var/lib/vz/template/iso/server.iso
