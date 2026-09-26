let authToken = localStorage.getItem('authToken');
let currentUserId = null;

const authView = document.getElementById('auth-view');
const dashboardView = document.getElementById('dashboard-view');
const adminSection = document.getElementById('admin-section');

function decodeToken(token) {
    const payload = token.split('.')[1];
    return JSON.parse(atob(payload));
}

async function showDashboard() {
    const claims = decodeToken(authToken);
    currentUserId = claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
    const role = claims["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];


    const response = await fetch(`/api/user/${currentUserId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
    });

    if (!response.ok) {
        logout();
        return;
    }

    const user = await response.json();
    document.getElementById('my-username').textContent = user.username;
    document.getElementById('my-email').textContent = user.email;
    document.getElementById('my-role').textContent = user.role;

    authView.classList.add('hidden');
    dashboardView.classList.remove('hidden');

    if (role === 'Admin') {
        adminSection.classList.remove('hidden');
        loadAllUsers();
    } else {
        adminSection.classList.add('hidden');
    }
}

function logout() {
    authToken = null;
    currentUserId = null;
    localStorage.removeItem('authToken');
    dashboardView.classList.add('hidden');
    authView.classList.remove('hidden');
}

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
        localStorage.setItem('authToken', authToken);
        messageEl.textContent = '';
        showDashboard();
        messageEl.textContent = "Logged in!";
    } else {
        messageEl.textContent = 'Invalid email or password.';
    }
});

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
        messageEl.textContent = 'Registered! You can now log in.';
    } else if (response.status === 409) {
        messageEl.textContent = 'A user with that email already exists.';
    } else {
        messageEl.textContent = 'Registration failed.';
    }
});

document.getElementById('logout-btn').addEventListener('click', logout);

document.getElementById('load-users-btn').addEventListener('click', loadAllUsers);

async function loadAllUsers() {
    const listEl = document.getElementById('users-list');
    listEl.innerHTML = '';

    const response = await fetch('/api/user', {
        headers: { 'Authorization': `Bearer ${authToken}` }
    });

    if (!response.ok) {
        listEl.innerHTML = '<li>Failed to load users.</li>';
        return;
    }

    const users = await response.json();
    users.forEach(u => {
        const li = document.createElement('li');
        li.textContent = `${u.username} (${u.email}) - ${u.role} `;

        if (u.role !== 'Admin') {
            const promoteBtn = document.createElement('button');
            promoteBtn.textContent = 'Make Admin';
            promoteBtn.addEventListener('click', () => promoteToAdmin(u.id, li));
            li.appendChild(promoteBtn);
        }

        listEl.appendChild(li);
    });
}

async function promoteToAdmin(userId, listItemEl) {
    const response = await fetch(`/api/user/${userId}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${authToken}`
        },
        body: JSON.stringify({ "role": 'Admin' })
    });

    if (response.ok) {
        loadAllUsers(); // refresh the whole list so the button disappears and role text updates
    } else {
        alert('Failed to promote user.');
    }
}
//skips straight to dashboard if already logged in
if (authToken) {
    showDashboard();
}