"""
One-off port-time reconciliation of the legacy KONGOR Plinko tier product lists against
the current NEXUS StoreItemsConfiguration.

Writes PlinkoTierProducts.json in this directory and prints a diff summary to stdout.
"""

import json
import pathlib
import sys

SCRIPT_DIR = pathlib.Path(__file__).parent
NEXUS_ROOT = SCRIPT_DIR.parents[2]
LEGACY_SEED = pathlib.Path(r"C:\Users\SADS-810\Source\KONGOR\KONGOR\Data\Seed\PlinkoTierProducts.json")
NEXUS_STORE = NEXUS_ROOT / "KONGOR.MasterServer" / "Configuration" / "Store" / "StoreItemsConfiguration.json"
OUTPUT = SCRIPT_DIR / "PlinkoTierProducts.json"


def load_legacy_tiers() -> dict[int, list[int]]:
    """Parse the legacy seed's pipe-delimited product lists into per-tier integer lists."""
    with open(LEGACY_SEED, "r", encoding="utf-8") as handle:
        raw = json.load(handle)
    tiers: dict[int, list[int]] = {}
    for entry in raw:
        tier_id = int(entry["TierId"])
        product_ids = [int(fragment) for fragment in entry["TierProducts"].split("|") if fragment.strip()]
        tiers[tier_id] = product_ids
    return tiers


def load_nexus_store() -> dict[int, dict]:
    with open(NEXUS_STORE, "r", encoding="utf-8") as handle:
        items = json.load(handle)
    return {int(item["ID"]): item for item in items}


def main() -> int:
    legacy_tiers = load_legacy_tiers()
    store = load_nexus_store()

    pruned: list[tuple[int, int, str]] = []
    surviving_by_tier: dict[int, list[int]] = {tier: [] for tier in sorted(legacy_tiers)}
    seen_in_any_tier: set[int] = set()

    for tier_id, product_ids in legacy_tiers.items():
        for product_id in product_ids:
            item = store.get(product_id)
            if item is None:
                pruned.append((tier_id, product_id, "missing from NEXUS store"))
                continue
            if not item.get("IsEnabled", False):
                pruned.append((tier_id, product_id, "disabled"))
                continue
            if not item.get("Purchasable", False):
                pruned.append((tier_id, product_id, "not purchasable"))
                continue
            surviving_by_tier[tier_id].append(product_id)
            seen_in_any_tier.add(product_id)

    brackets: dict[int, tuple[int, int]] = {}
    for tier_id, survivors in surviving_by_tier.items():
        gold_costs = [store[pid]["GoldCost"] for pid in survivors if store[pid]["GoldCost"] > 0]
        if gold_costs:
            brackets[tier_id] = (min(gold_costs), max(gold_costs))

    sorted_tiers_by_value = sorted(brackets.keys())

    augmented: list[tuple[int, int, int]] = []
    for product_id, item in store.items():
        if product_id in seen_in_any_tier:
            continue
        if not item.get("IsEnabled", False) or not item.get("Purchasable", False):
            continue
        gold_cost = item.get("GoldCost", 0)

        # Free items (GoldCost == 0) have no price signal, so route them to Bronze as the catch-all tier.
        if gold_cost <= 0:
            surviving_by_tier[4].append(product_id)
            augmented.append((4, product_id, gold_cost))
            continue

        chosen_tier = None
        for tier_id in sorted_tiers_by_value:
            lo, hi = brackets[tier_id]
            if lo <= gold_cost <= hi:
                chosen_tier = tier_id
                break
        if chosen_tier is None:
            max_bracket_high = max(hi for _, hi in brackets.values())
            min_bracket_low = min(lo for lo, _ in brackets.values())
            if gold_cost > max_bracket_high:
                chosen_tier = 1
            elif gold_cost < min_bracket_low:
                chosen_tier = 4
            else:
                chosen_tier = 4

        surviving_by_tier[chosen_tier].append(product_id)
        augmented.append((chosen_tier, product_id, gold_cost))

    output: list[dict] = []
    for tier_id in sorted(surviving_by_tier):
        output.append({
            "TierID": tier_id,
            "ProductIDs": sorted(set(surviving_by_tier[tier_id]))
        })

    with open(OUTPUT, "w", encoding="utf-8") as handle:
        json.dump(output, handle, indent=4)
        handle.write("\n")

    print("=== Reconciliation Summary ===")
    for tier_id in sorted(surviving_by_tier):
        print(f"Tier {tier_id}: {len(set(surviving_by_tier[tier_id]))} products")
    print(f"\nPruned: {len(pruned)} entries")
    pruned_by_reason: dict[str, int] = {}
    for _, _, reason in pruned:
        pruned_by_reason[reason] = pruned_by_reason.get(reason, 0) + 1
    for reason, count in sorted(pruned_by_reason.items()):
        print(f"  {reason}: {count}")
    print(f"\nAugmented: {len(augmented)} new products assigned to tiers")
    aug_by_tier: dict[int, int] = {}
    for tier_id, _, _ in augmented:
        aug_by_tier[tier_id] = aug_by_tier.get(tier_id, 0) + 1
    for tier_id in sorted(aug_by_tier):
        print(f"  Tier {tier_id}: +{aug_by_tier[tier_id]}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
