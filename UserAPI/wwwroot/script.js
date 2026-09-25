let authToken = null;

document.getElementById('register-form').addEventListener('submit', async (e) => {
    e.preventDefault();

    const email = document.getElementById('register-email').value;
    const username = document.getElementById('register-username').value;
    const password = document.getElementById('register-password').value;

    const response = await fetch('/api/user', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, username, password })
    });

    const messageEl = document.getElementById('register-message');
    if (response.ok) {
        messageEl.textContent = 'Registered successfully! You can now log in.';
    } else if (response.status === 409) {
        messageEl.textContent = 'A user with that email already exists.';
    } else {
        messageEl.textContent = 'Registration failed.';
    }
});

document.getElementById('login-form').addEventListener('submit', async (e) => {
    e.preventDefault();

    const email = document.getElementById('login-email').value;
    const password = document.getElementById('login-password').value;

    const response = await fetch('/api/user/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
    });

    const messageEl = document.getElementById('login-message');
    if (response.ok) {
        const data = await response.json();
        authToken = data.token;
        messageEl.textContent = 'Logged in successfully.';
    } else {
        messageEl.textContent = 'Invalid email or password.';
    }
});

document.getElementById('load-users-btn').addEventListener('click', async () => {
    const listEl = document.getElementById('users-list');
    listEl.innerHTML = '';

    if (!authToken) {
        listEl.innerHTML = '<li>Please log in first.</li>';
        return;
    }

    const response = await fetch('/api/user', {
        headers: { 'Authorization': `Bearer ${authToken}` }
    });

    if (response.status === 403) {
        listEl.innerHTML = '<li>You must be an Admin to view this.</li>';
        return;
    }
    if (!response.ok) {
        listEl.innerHTML = '<li>Failed to load users.</li>';
        return;
    }

    const users = await response.json();
    users.forEach(u => {
        const li = document.createElement('li');
        li.textContent = `${u.username} (${u.email}) - ${u.role}`;
        listEl.appendChild(li);
    });
});