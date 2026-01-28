document.addEventListener('DOMContentLoaded', function () {
    const form = document.querySelector('form');
    if (!form) return;

    const environment = form.getAttribute('data-environment');
    const isDevelopment = environment === 'Development';

    const passwordInput = document.getElementById('Password');
    const ignInput = document.getElementById('IGN');

    // Requirements Definitions
    const passwordRequirements = [
        { id: 'req-length', text: 'At least 8 characters', check: (val) => val.length >= 8 },
        { id: 'req-upper', text: 'At least one uppercase letter', check: (val) => /[A-Z]/.test(val) },
        { id: 'req-lower', text: 'At least one lowercase letter', check: (val) => /[a-z]/.test(val) },
        { id: 'req-digit', text: 'At least one number', check: (val) => /[0-9]/.test(val) },
        { id: 'req-special', text: 'At least one special character', check: (val) => /[^A-Za-z0-9]/.test(val) }
    ];

    const ignRequirements = [
        { id: 'req-ign-chars', text: 'Allowed characters: Letters, Numbers, -, _, `', check: (val) => /^[a-zA-Z0-9\-_`]*$/.test(val) && val.length > 0 }
    ];

    // If Development, we technically don't enforce these strictly in the UI blocking submission,
    // but the user requested "relaxed for devs".
    // If strict compliance is "relaxed", maybe we just don't show the red errors or the list?
    // However, the user also said "new user knows instantly...".
    // Let's show the requirements but maybe mark them as optional or just validate anyway
    // so the dev *knows* what a prod password looks like, even if the backend accepts 'a'.
    // Actually, to match the backend behavior:
    // In Dev: Backend accepts anything. UI should probably be permissive too.
    // In Prod: Backend requires strict. UI must be strict.

    // Implementation: Always show feedback, but visual cues might differ?
    // Simpler approach: Just validate. If Dev, maybe we don't block submit (client-side validation usually blocks).
    // The ASP.NET Core MVC validation scripts (jquery.validate.unobtrusive) will run based on DataAnnotations on the Model.
    // This JS is for *instant visual feedback* (the "checklist").

    function createRequirementList(requirements, containerId) {
        const container = document.createElement('div');
        container.id = containerId;
        container.className = 'mt-2 space-y-1 text-xs text-slate-500 transition-all duration-300';

        // If Development, maybe hide this entirely or add a "Dev Mode: Relaxed" badge?
        // User asked for "relaxed for devs". Let's assume that means we don't annoy them with red text.
        if (isDevelopment) {
            const devBadge = document.createElement('div');
            devBadge.className = 'text-blue-400 mb-1 font-semibold';
            devBadge.textContent = 'Development Mode: Validation Relaxed';
            container.appendChild(devBadge);
            // We still append the list below so they *can* see if they meet prod standards if they want.
        }

        const list = document.createElement('ul');
        requirements.forEach(req => {
            const li = document.createElement('li');
            li.id = req.id;
            li.className = 'flex items-center gap-2 transition-colors duration-200';
            li.innerHTML = `
                <svg class="w-3 h-3 opacity-0 transition-opacity duration-200 check-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path></svg>
                <span>${req.text}</span>
            `;
            list.appendChild(li);
        });
        container.appendChild(list);
        return container;
    }

    if (passwordInput) {
        const reqList = createRequirementList(passwordRequirements, 'password-requirements-list');
        passwordInput.parentNode.appendChild(reqList);

        passwordInput.addEventListener('input', function() {
            const val = this.value;
            passwordRequirements.forEach(req => {
                const el = document.getElementById(req.id);
                const isValid = req.check(val);
                const icon = el.querySelector('.check-icon');

                if (isValid) {
                    el.classList.remove('text-slate-500');
                    el.classList.add('text-green-400');
                    icon.classList.remove('opacity-0');
                } else {
                    el.classList.add('text-slate-500');
                    el.classList.remove('text-green-400');
                    icon.classList.add('opacity-0');
                }
            });
        });
    }

    if (ignInput) {
        const reqList = createRequirementList(ignRequirements, 'ign-requirements-list');
        ignInput.parentNode.appendChild(reqList);

        ignInput.addEventListener('input', function() {
            const val = this.value;
            ignRequirements.forEach(req => {
                const el = document.getElementById(req.id);
                const isValid = req.check(val);
                const icon = el.querySelector('.check-icon');

                if (isValid) {
                    el.classList.remove('text-slate-500');
                    el.classList.add('text-green-400');
                    icon.classList.remove('opacity-0');
                } else {
                    // Only show "invalid" color if they have typed something invalid
                    // otherwise default state
                    if (val.length > 0 && !isValid) {
                         el.classList.add('text-red-400'); // Explicit error for bad chars
                         el.classList.remove('text-slate-500');
                    } else {
                        el.classList.add('text-slate-500');
                        el.classList.remove('text-red-400');
                    }
                    el.classList.remove('text-green-400');
                    icon.classList.add('opacity-0');
                }
            });
        });
    }
});
