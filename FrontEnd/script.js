// Global variables
let isAuthenticated = false;
let currentView = 'home';
let currentUser = null;
let users = {};
let tasks = [];
let requests = [];
let documents = [];
let notifications = [];
let profilePicUrl = 'default-profile.jpg';

// Initialize app
$(document).ready(function() {
  // Load data from localStorage with error handling
  try {
    users = JSON.parse(localStorage.getItem('users') || '{}');//for object
    tasks = JSON.parse(localStorage.getItem('tasks') || '[]');//for arrays
    requests = JSON.parse(localStorage.getItem('requests') || '[]');
    documents = JSON.parse(localStorage.getItem('documents') || '[]');
    notifications = JSON.parse(localStorage.getItem('notifications') || '[]');
    profilePicUrl = localStorage.getItem('profilePicUrl') || 'default-profile.jpg';
  } catch (e) {
    console.error('Failed to load localStorage:', e);
    showToast('Error loading data. Some features may not work.', 'danger');
  }

  initializeDemoData();
  showView('home');

  // Attach form submit handlers
  $('#login-form').on('submit', handleLogin);
  $('#register-form').on('submit', function(event) {
    event.preventDefault();
    if (!validateRegistrationForm()) return;
    handleRegister(event);
  });
  $('#request-form').on('submit', handleRequest);
});

// View management
function showView(viewId) {
  // Prevent unauthorized access
  if (['dashboard', 'request'].includes(viewId) && !isAuthenticated) {
    showToast('Please login first.', 'warning');
    viewId = 'login';
  }

  // Hide all views using 'hidden' class
  const views = ['login', 'register', 'home', 'request', 'dashboard'];
  views.forEach(view => {
    const $view = $(`#${view}-view`);
    if ($view.length) $view.addClass('hidden');
  });

  // Show selected view
  const $view = $(`#${viewId}-view`);
  if ($view.length) {
    $view.removeClass('hidden');
    currentView = viewId;
    updateNavigation();
    if (viewId === 'dashboard') {
      loadDashboardSection('home');
    }
  } else {
    console.error(`View #${viewId}-view not found`);
    showToast('Error loading view.', 'danger');
  }
}

//updatenavigation
function updateNavigation() {
  const $navItems = $('#nav-items');
  if (!$navItems.length) {
    console.error('#nav-items not found');
    return;
  }

  if (isAuthenticated) {
    $navItems.html(`
      <li class="nav-item"><a class="nav-link ${currentView === 'dashboard' ? 'active' : ''}" href="#" data-view="dashboard">Dashboard</a></li>
      <li class="nav-item"><a class="nav-link" href="#" data-view="request">Submit Request</a></li>
      <li class="nav-item"><a class="nav-link" href="#" data-action="logout">Logout</a></li>
    `);
  } else {
    $navItems.html(`
      <li class="nav-item"><a class="nav-link ${currentView === 'login' ? 'active' : ''}" href="#" data-view="login">Login</a></li>
      <li class="nav-item"><a class="nav-link ${currentView === 'register' ? 'active' : ''}" href="#" data-view="register">Register</a></li>
    `);
  }
}

// Password toggle
function togglePassword(inputId, element) {
  const $input = $(`#${inputId}`);
  const $icon = $(element).find('i');
  if ($input.length && $icon.length) {
    if ($input.attr('type') === 'password') {
      $input.attr('type', 'text');
      $icon.removeClass('fa-eye-slash').addClass('fa-eye');
    } else {
      $input.attr('type', 'password');
      $icon.removeClass('fa-eye').addClass('fa-eye-slash');
    }
  }
}

// Authentication handlers
function handleLogin(event) {
  event.preventDefault();
  if (!validateLoginForm()) return;

  const email = $('#login-email').val().trim();
  const password = $('#login-password').val();

  if (users[email] && users[email].password === password) {
    isAuthenticated = true;
    currentUser = users[email];
    const $userNameDisplay = $('#userNameDisplay');
    if ($userNameDisplay.length) $userNameDisplay.text(currentUser.name);
    showToast(`Login successful! Welcome, ${currentUser.name}`, 'success');
    setTimeout(() => showView('dashboard'), 1000);
  } else {
    showToast('Invalid email or password.', 'danger');
  }
}

//register
function handleRegister(event) {
  console.log('handleRegister called');
  event.preventDefault();
  console.log('Pre-validation check result:', validateRegistrationForm());
  if (!validateRegistrationForm()) {
    console.log('Validation failed in handleRegister, should stop');
    return;
  }
  const name = $('#name').val().trim();
  const email = $('#reg-email').val().trim();
  const password = $('#reg-password').val();
  const confirmPassword = $('#confirm-password').val();

  if (password !== confirmPassword) {
    showToast('Passwords do not match.', 'danger');
    return;
  }

  if (users[email]) {
    showToast('Email already registered.', 'warning');
    return;
  }

  users[email] = {
    name,
    email,
    password,
    registrationDate: new Date().toISOString(),
    phone: '',
    department: ''
  };
  try {
    localStorage.setItem('users', JSON.stringify(users));
  } catch (e) {
    showToast('Error saving user data.', 'danger');
    return;
  }
  showToast('Registration successful! Please login.', 'success');
  $('#register-form')[0].reset();
  setTimeout(() => showView('login'), 1500);
}

//handlelogout
function handleLogout() {
  isAuthenticated = false;
  currentUser = null;
  profilePicUrl = 'default-profile.jpg';
  try {
    localStorage.setItem('profilePicUrl', profilePicUrl);
  } catch (e) {
    showToast('Error resetting profile picture.', 'danger');
  }
  showToast('Logged out successfully.', 'info');
  showView('home');
}

// Request handler
function handleRequest(event) {
  event.preventDefault();
  if (!validateRequestForm()) return;

  const requestData = {
    id: requests.length + 1,
    employeeName: $('#employee-name').val().trim(),
    employeeId: $('#employee-id').val().trim(),
    department: $('#department').val(),
    description: $('#request-description').val().trim(),
    priority: $('#request-priority').val(),
    submissionDate: new Date().toISOString(),
    status: 'Pending'
  };

  requests.push(requestData);
  notifications.push({
    id: notifications.length + 1,
    message: `Request #${requestData.id} submitted`,
    icon: 'fas fa-check-circle',
    type: 'success',
    timestamp: new Date().toISOString(),
    read: false
  });

  try {
    localStorage.setItem('requests', JSON.stringify(requests));
    localStorage.setItem('notifications', JSON.stringify(notifications));
  } catch (e) {
    showToast('Error saving request.', 'danger');
    return;
  }

  showToast('Request submitted!', 'success');
  $('#request-form')[0].reset();
  setTimeout(() => {
    if (isAuthenticated) {
      showView('dashboard');
      loadDashboardSection('requests');
    } else {
      showView('home');
    }
  }, 1500);
}

// Dashboard section loader
function loadDashboardSection(sectionId) {
  const $sidebarLinks = $('.sidebar .nav-link');
  const $contentArea = $('#contentArea');
  if (!$contentArea.length || !$sidebarLinks.length) {
    console.error('Dashboard elements not found');
    showToast('Error loading dashboard.', 'danger');
    return;
  }

  $sidebarLinks.removeClass('active');
  const $activeLink = $(`.sidebar .nav-link[data-section="${sanitizeInput(sectionId)}"]`);
  if ($activeLink.length) $activeLink.addClass('active');

  switch (sectionId) {
    case 'home':
      $contentArea.html(`
        <div class="dashboard-card">
          <h2>Overview</h2>
          <div class="row">
            <div class="col-md-3"><div class="stat-card"><h3>${tasks.length}</h3><p>Tasks</p></div></div>
            <div class="col-md-3"><div class="stat-card"><h3>${requests.filter(r => r.status === 'Pending').length}</h3><p>Pending Requests</p></div></div>
            <div class="col-md-3"><div class="stat-card"><h3>${notifications.filter(n => !n.read).length}</h3><p>Unread Notifications</p></div></div>
            <div class="col-md-3"><div class="stat-card"><h3>${documents.length}</h3><p>Documents</p></div></div>
          </div>
        </div>
      `);
      break;

    case 'profile':
      if (!isAuthenticated) {
        showView('login');
        return;
      }
      $contentArea.html(`
        <div class="dashboard-card">
          <h2>Profile</h2>
          <div class="text-center mb-3">
            <img id="profile-pic-preview" src="${profilePicUrl}" alt="Profile Picture" class="img-thumbnail rounded-circle" style="width: 120px; height: 120px; object-fit: cover;">
            <input type="file" id="profile-pic-input" accept="image/*" class="form-control mt-2">
            <button class="btn btn-primary mt-2" data-action="upload-pic">Upload Picture</button>
          </div>
          <div class="row">
            <div class="col-md-6">
              <div class="form-group mb-2"><label>Name</label><input class="form-control" value="${sanitizeInput(currentUser.name)}" readonly></div>
              <div class="form-group mb-2"><label>Email</label><input class="form-control" value="${sanitizeInput(currentUser.email)}" readonly></div>
              <div class="form-group mb-2"><label>Phone</label><input class="form-control" id="profile-phone" value="${sanitizeInput(currentUser.phone || '')}" placeholder="+1 123-456-7890"></div>
              <div class="form-group mb-2"><label>Department</label><input class="form-control" id="profile-department" value="${sanitizeInput(currentUser.department || '')}" placeholder="e.g., IT"></div>
            </div>
          </div>
          <button class="btn btn-primary" data-action="update-profile">Update Profile</button>
        </div>
      `);
      break;

    case 'tasks':
      $contentArea.html(`
        <div class="dashboard-card">
          <h2>Tasks</h2>
          <div class="row mb-3">
            <div class="col-md-6"><input class="form-control" id="new-task-title" placeholder="Task title"></div>
            <div class="col-md-6"><input class="form-control" id="new-task-description" placeholder="Description"></div>
          </div>
          <button class="btn btn-primary mb-3" data-action="add-task">Add Task</button>
          <div class="row">
            ${tasks.map(task => `
              <div class="col-md-4 mb-3">
                <div class="card">
                  <div class="card-body">
                    <h5>${sanitizeInput(task.title)}</h5>
                    <p>${sanitizeInput(task.description)}</p>
                    <span class="badge bg-${task.status === 'Completed' ? 'success' : task.status === 'In Progress' ? 'warning' : 'secondary'}">${task.status}</span>
                    <div class="mt-2">
                      <button class="btn btn-sm btn-outline-primary" data-action="edit-task" data-id="${task.id}">Edit</button>
                      <button class="btn btn-sm btn-outline-danger" data-action="delete-task" data-id="${task.id}">Delete</button>
                    </div>
                  </div>
                </div>
              </div>
            `).join('') || '<p>No tasks.</p>'}
          </div>
        </div>
      `);
      break;

    case 'requests':
      $contentArea.html(`
        <div class="dashboard-card">
          <h2>Requests</h2>
          <table class="table">
            <thead><tr><th>ID</th><th>Description</th><th>Priority</th><th>Status</th><th>Actions</th></tr></thead>
            <tbody>
              ${requests.map(req => `
                <tr>
                  <td>REQ-${req.id}</td>
                  <td>${sanitizeInput(req.description)}</td>
                  <td><span class="badge bg-${req.priority === 'High' ? 'danger' : req.priority === 'Medium' ? 'warning' : 'success'}">${req.priority}</span></td>
                  <td><span class="badge bg-${req.status === 'Completed' ? 'success' : req.status === 'In Progress' ? 'info' : 'secondary'}">${req.status}</span></td>
                  <td>
                    <button class="btn btn-sm btn-outline-primary" data-action="edit-request" data-id="${req.id}">Edit</button>
                    <button class="btn btn-sm btn-outline-danger" data-action="delete-request" data-id="${req.id}">Delete</button>
                  </td>
                </tr>
              `).join('') || '<tr><td colspan="5">No requests.</td></tr>'}
            </tbody>
          </table>
          <button class="btn btn-primary" data-view="request">New Request</button>
        </div>
      `);
      break;

    case 'notifications':
      $contentArea.html(`
        <div class="dashboard-card">
          <h2>Notifications</h2>
          <button class="btn btn-outline-primary mb-2" data-action="clear-notifications">Clear All</button>
          <div>
            ${notifications.map(n => `
              <div class="p-2 mb-2 border ${n.read ? 'border-secondary' : 'border-primary'}" data-id="${n.id}">
                <i class="${n.icon} text-${n.type} me-2"></i>
                ${sanitizeInput(n.message)} (${formatDate(n.timestamp)})
              </div>
            `).join('') || '<p>No notifications.</p>'}
          </div>
        </div>
      `);
      break;

    case 'documents':
      $contentArea.html(`
        <div class="dashboard-card">
          <h2>Documents</h2>
          <input type="file" id="document-upload" accept=".pdf,.doc,.docx,.xlsx" class="form-control mb-2">
          <button class="btn btn-primary mb-2" data-action="upload-document">Upload</button>
          <div class="row">
            ${documents.map(doc => `
              <div class="col-md-4 mb-3">
                <div class="card">
                  <div class="card-body">
                    <i class="fas fa-file-${doc.type} fa-2x mb-2"></i>
                    <h6>${sanitizeInput(doc.name)}</h6>
                    <button class="btn btn-sm btn-outline-danger" data-action="delete-document" data-id="${doc.id}">Delete</button>
                  </div>
                </div>
              </div>
            `).join('') || '<p>No documents.</p>'}
          </div>
        </div>
      `);
      break;

    case 'settings':
      const settings = JSON.parse(localStorage.getItem('settings') || '{}');
      $contentArea.html(`
        <div class="dashboard-card">
          <h2>Settings</h2>
          <div class="form-check mb-2">
            <input class="form-check-input" type="checkbox" id="email-notifications" ${settings.emailNotifications ? 'checked' : ''}>
            <label class="form-check-label">Email Notifications</label>
          </div>
          <button class="btn btn-primary" data-action="save-settings">Save</button>
        </div>
      `);
      break;

    default:
      $contentArea.html('<div class="alert alert-warning">Section not found.</div>');
  }
}

// Toast notification
function showToast(message, type = 'info') {
  $('.toast-notification').remove();
  const $toast = $(`
    <div class="toast-notification alert alert-${type} alert-dismissible fade show" style="position: fixed; top: 20px; right: 20px; z-index: 9999; max-width: 350px;">
      ${sanitizeInput(message)}
      <button type="button" class="btn-close" data-action="close-toast"></button>
    </div>
  `);
  $('body').append($toast);
  setTimeout(() => $toast.remove(), 5000);
}

// Form validation
function validateEmail(email) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

function validatePassword(password) {
  return /^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$/.test(password);
}

//registration form validation
function validateRegistrationForm() {
  const name = $('#name').val().trim();
  const email = $('#reg-email').val().trim();
  const password = $('#reg-password').val();
  const confirmPassword = $('#confirm-password').val();
  $('.form-control').removeClass('is-invalid'); // Clear all invalid states
  let isValid = true;
  const errors = []; // Collect errors for single toast

  if (!name || name.length < 2) {
    errors.push('Name must be at least 2 characters.');
    $('#name').addClass('is-invalid');
    isValid = false;
  }
  if (!validateEmail(email)) {
    errors.push('Invalid email.');
    $('#reg-email').addClass('is-invalid');
    isValid = false;
  }
  if (!validatePassword(password)) {
    errors.push('Password must be 8+ characters with letters and numbers.');
    $('#reg-password').addClass('is-invalid');
    isValid = false;
  }
  if (password !== confirmPassword) {
    errors.push('Passwords do not match.');
    $('#confirm-password').addClass('is-invalid');
    isValid = false;
  }

  // Show a single toast with all errors if any
  if (errors.length > 0) {
    showToast(errors.join(' '), 'danger');
  }

  return isValid;
}

//loginform
function validateLoginForm() {
  const email = $('#login-email').val().trim();
  const password = $('#login-password').val();
  $('.form-control').removeClass('is-invalid');
  let isValid = true;
  if (!validateEmail(email)) {
    $('#login-email').addClass('is-invalid');
    showToast('Invalid email.', 'danger');
    isValid = false;
  }
  if (!password) {
    $('#login-password').addClass('is-invalid');
    showToast('Password required.', 'danger');
    isValid = false;
  }
  return isValid;
}

//requestform
function validateRequestForm() {
  const employeeName = $('#employee-name').val().trim();
  const employeeId = $('#employee-id').val().trim();
  const department = $('#department').val();
  const description = $('#request-description').val().trim();
  const priority = $('#request-priority').val();
  $('.form-control, .form-select').removeClass('is-invalid');
  let isValid = true;
  if (!employeeName || employeeName.length < 2) {
    $('#employee-name').addClass('is-invalid');
    showToast('Name must be at least 2 characters.', 'danger');
    isValid = false;
  }
  if (!employeeId) {
    $('#employee-id').addClass('is-invalid');
    showToast('Employee ID required.', 'danger');
    isValid = false;
  }
  if (!department) {
    $('#department').addClass('is-invalid');
    showToast('Department required.', 'danger');
    isValid = false;
  }
  if (!description || description.length < 10) {
    $('#request-description').addClass('is-invalid');
    showToast('Description must be 10+ characters.', 'danger');
    isValid = false;
  }
  if (!priority) {
    $('#request-priority').addClass('is-invalid');
    showToast('Priority required.', 'danger');
    isValid = false;
  }
  return isValid;
}

// Initialize demo data
function initializeDemoData() {
  if (!Object.keys(users).length) {
    users['demo@example.com'] = {
      name: 'Demo User',
      email: 'demo@example.com',
      password: 'password123',
      registrationDate: new Date().toISOString(),
      phone: '',
      department: ''
    };
    localStorage.setItem('users', JSON.stringify(users));
  }
  if (!tasks.length) {
    tasks = [
      { id: 1, title: 'Review', description: 'Q1 reports', status: 'In Progress', createdDate: new Date().toISOString() }
    ];
    localStorage.setItem('tasks', JSON.stringify(tasks));
  }
  if (!requests.length) {
    requests = [
      { id: 1, employeeName: 'Demo', employeeId: '123', department: 'IT', description: 'Fix laptop', priority: 'Medium', submissionDate: new Date().toISOString(), status: 'Pending' }
    ];
    localStorage.setItem('requests', JSON.stringify(requests));
  }
  if (!notifications.length) {
    notifications = [
      { id: 1, message: 'System update', icon: 'fas fa-info-circle', type: 'info', timestamp: new Date().toISOString(), read: false }
    ];
    localStorage.setItem('notifications', JSON.stringify(notifications));
  }
}

// Utility functions
function formatDate(dateString) {
  return new Date(dateString).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
}

function sanitizeInput(input) {
  return $('<div>').text(input).html();
}

// Event handlers
//navigation link click handler
$(document).on('click', '#nav-items .nav-link', function(e) {
  e.preventDefault();
  const view = $(this).data('view');
  const action = $(this).data('action');
  if (view) showView(view);
  if (action === 'logout') handleLogout();
});

//password toggle handler
$(document).on('click', '.toggle-password', function() {
  const inputId = $(this).data('input');
  togglePassword(inputId, this);
});

//close toast handler
$(document).on('click', '[data-action="close-toast"]', function() {
  $(this).parent().remove();
});


//upload profile picture handler
$(document).on('click', '[data-action="upload-pic"]', function() {
  const file = $('#profile-pic-input')[0].files[0];
  if (!file) {
    showToast('Select an image.', 'warning');
    return;
  }
  if (file.size > 2_000_000) {
    showToast('Image exceeds 2MB.', 'danger');
    return;
  }
  const reader = new FileReader();
  reader.onload = e => {
    profilePicUrl = e.target.result;
    localStorage.setItem('profilePicUrl', profilePicUrl);
    $('#profile-pic-preview').attr('src', profilePicUrl);
    showToast('Picture updated!', 'success');
  };
  reader.readAsDataURL(file);
});

//update profile handler
$(document).on('click', '[data-action="update-profile"]', function() {
  const phone = $('#profile-phone').val().trim();
  const department = $('#profile-department').val().trim();
  if (!phone.match(/^\+?\d{10,12}$/)) {
    showToast('Invalid phone number.', 'danger');
    return;
  }
  if (!department || department.length < 2) {
    showToast('Invalid department.', 'danger');
    return;
  }
  users[currentUser.email] = { ...currentUser, phone, department };
  localStorage.setItem('users', JSON.stringify(users));
  currentUser = users[currentUser.email];
  showToast('Profile updated!', 'success');
});

//add task handler
$(document).on('click', '[data-action="add-task"]', function() {
  const title = $('#new-task-title').val().trim();
  const description = $('#new-task-description').val().trim();
  if (!title || !description) {
    showToast('Title and description required.', 'danger');
    return;
  }
  tasks.push({
    id: tasks.length + 1,
    title,
    description,
    status: 'Pending',
    createdDate: new Date().toISOString()
  });
  localStorage.setItem('tasks', JSON.stringify(tasks));
  $('#new-task-title, #new-task-description').val('');
  showToast('Task added!', 'success');
  loadDashboardSection('tasks');
});

//edit task handler
$(document).on('click', '[data-action="edit-task"]', function() {
  const taskId = parseInt($(this).data('id'));
  const task = tasks.find(t => t.id === taskId);
  $('#contentArea').prepend(`
    <div class="dashboard-card mb-3">
      <h3>Edit Task</h3>
      <input class="form-control mb-2" id="edit-task-title" value="${sanitizeInput(task.title)}">
      <input class="form-control mb-2" id="edit-task-description" value="${sanitizeInput(task.description)}">
      <select class="form-select mb-2" id="edit-task-status">
        <option value="Pending" ${task.status === 'Pending' ? 'selected' : ''}>Pending</option>
        <option value="In Progress" ${task.status === 'In Progress' ? 'selected' : ''}>In Progress</option>
        <option value="Completed" ${task.status === 'Completed' ? 'selected' : ''}>Completed</option>
      </select>
      <button class="btn btn-primary" data-action="save-task" data-id="${taskId}">Save</button>
      <button class="btn btn-outline-secondary ms-2" data-action="cancel-edit">Cancel</button>
    </div>
  `);
});

//save task handler
$(document).on('click', '[data-action="save-task"]', function() {
  const taskId = parseInt($(this).data('id'));
  const title = $('#edit-task-title').val().trim();
  const description = $('#edit-task-description').val().trim();
  const status = $('#edit-task-status').val();
  if (!title || !description) {
    showToast('Title and description required.', 'danger');
    return;
  }
  tasks = tasks.map(t => t.id === taskId ? { ...t, title, description, status } : t);
  localStorage.setItem('tasks', JSON.stringify(tasks));
  showToast('Task updated!', 'success');
  loadDashboardSection('tasks');
});

//delete task handler
$(document).on('click', '[data-action="delete-task"]', function() {
  const taskId = parseInt($(this).data('id'));
  tasks = tasks.filter(t => t.id !== taskId);
  localStorage.setItem('tasks', JSON.stringify(tasks));
  showToast('Task deleted!', 'success');
  loadDashboardSection('tasks');
});


//edit request handler
$(document).on('click', '[data-action="edit-request"]', function() {
  const reqId = parseInt($(this).data('id'));
  const req = requests.find(r => r.id === reqId);
  $('#contentArea').prepend(`
    <div class="dashboard-card mb-3">
      <h3>Edit Request</h3>
      <input class="form-control mb-2" id="edit-request-description" value="${sanitizeInput(req.description)}">
      <select class="form-select mb-2" id="edit-request-priority">
        <option value="Low" ${req.priority === 'Low' ? 'selected' : ''}>Low</option>
        <option value="Medium" ${req.priority === 'Medium' ? 'selected' : ''}>Medium</option>
        <option value="High" ${req.priority === 'High' ? 'selected' : ''}>High</option>
      </select>
      <button class="btn btn-primary" data-action="save-request" data-id="${reqId}">Save</button>
      <button class="btn btn-outline-secondary ms-2" data-action="cancel-edit">Cancel</button>
    </div>
  `);
});

//save request handler
$(document).on('click', '[data-action="save-request"]', function() {
  const reqId = parseInt($(this).data('id'));
  const description = $('#edit-request-description').val().trim();
  const priority = $('#edit-request-priority').val();
  if (!description || description.length < 10) {
    showToast('Description must be 10+ characters.', 'danger');
    return;
  }
  requests = requests.map(r => r.id === reqId ? { ...r, description, priority } : r);
  localStorage.setItem('requests', JSON.stringify(requests));
  showToast('Request updated!', 'success');
  loadDashboardSection('requests');
});

//delete request handler
$(document).on('click', '[data-action="delete-request"]', function() {
  const reqId = parseInt($(this).data('id'));
  requests = requests.filter(r => r.id !== reqId);
  localStorage.setItem('requests', JSON.stringify(requests));
  showToast('Request deleted!', 'success');
  loadDashboardSection('requests');
});

//cancel edit handler
$(document).on('click', '[data-action="cancel-edit"]', function() {
  loadDashboardSection('tasks'); // Adjust based on context
});


//clear notificaions handler
$(document).on('click', '[data-action="clear-notifications"]', function() {
  notifications = [];
  localStorage.setItem('notifications', JSON.stringify(notifications));
  showToast('Notifications cleared!', 'success');
  loadDashboardSection('notifications');
});

//upload document handler
$(document).on('click', '[data-action="upload-document"]', function() {
  const file = $('#document-upload')[0].files[0];
  if (!file) {
    showToast('Select a document.', 'warning');
    return;
  }
  if (file.size > 5_000_000) {
    showToast('Document exceeds 5MB.', 'danger');
    return;
  }
  const reader = new FileReader();
  reader.onload = e => {
    documents.push({
      id: documents.length + 1,
      name: file.name,
      type: file.name.split('.').pop().toLowerCase(),
      data: e.target.result,
      modified: new Date().toISOString()
    });
    localStorage.setItem('documents', JSON.stringify(documents));
    showToast('Document uploaded!', 'success');
    loadDashboardSection('documents');
  };
  reader.readAsDataURL(file);
});

//delete document handler
$(document).on('click', '[data-action="delete-document"]', function() {
  const docId = parseInt($(this).data('id'));
  documents = documents.filter(d => d.id !== docId);
  localStorage.setItem('documents', JSON.stringify(documents));
  showToast('Document deleted!', 'success');
  loadDashboardSection('documents');
});

//save settings handler
$(document).on('click', '[data-action="save-settings"]', function() {
  const settings = {
    emailNotifications: $('#email-notifications').is(':checked')
  };
  localStorage.setItem('settings', JSON.stringify(settings));
  showToast('Settings saved!', 'success');
});

$(document).on('click', '[data-view]', function(e) {
  e.preventDefault();
  console.log('Clicked view:', $(this).data('view'));
  const view = $(this).data('view');
  if (view) showView(view);
});