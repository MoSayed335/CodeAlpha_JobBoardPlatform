// API Configuration
const API_BASE = '/api';

// Application State
let state = {
    candidates: [],
    employers: [],
    activeCandidateId: null,
    activeEmployerId: null,
    jobListings: [],
    selectedJobForDetails: null,
    selectedJobForApplicants: null,
    notifications: [],
    adminTab: 'candidates' // candidates | employers
};

// On Page Load
document.addEventListener("DOMContentLoaded", () => {
    // Initial data fetch
    initApp();
});

async function initApp() {
    await Promise.all([
        loadCandidates(),
        loadEmployers()
    ]);
    
    // Load default listings
    await loadJobListings();
    
    // Refresh Icons
    lucide.createIcons();
}

// ==========================================
// PORTAL & TAB SWITCHING
// ==========================================
function switchTab(tabName) {
    // Toggle active classes on tab buttons
    document.querySelectorAll('.nav-tab').forEach(btn => btn.classList.remove('active'));
    document.getElementById(`tab-${tabName}`).classList.add('active');

    // Toggle active classes on sections
    document.querySelectorAll('.portal-section').forEach(sec => sec.classList.remove('active'));
    document.getElementById(`content-${tabName}`).classList.add('active');

    // Portal specific loads
    if (tabName === 'candidate') {
        loadJobListings();
        if (state.activeCandidateId) {
            loadCandidateApplications(state.activeCandidateId);
        }
    } else if (tabName === 'employer') {
        if (state.activeEmployerId) {
            loadEmployerJobListings();
            loadEmployerNotifications(state.activeEmployerId);
        }
    } else if (tabName === 'admin') {
        loadAdminData();
    }
    
    lucide.createIcons();
}

function switchAdminSubTab(subTabName) {
    document.querySelectorAll('.admin-tab').forEach(btn => btn.classList.remove('active'));
    document.getElementById(`admin-subtab-${subTabName}`).classList.add('active');

    document.querySelectorAll('.admin-subcontent').forEach(sec => sec.classList.remove('active'));
    document.getElementById(`admin-content-${subTabName}`).classList.add('active');
    
    state.adminTab = subTabName;
    loadAdminData();
}

// ==========================================
// CANDIDATES LOGIC
// ==========================================
async function loadCandidates() {
    try {
        const res = await fetch(`${API_BASE}/candidates`);
        state.candidates = await res.json();
        populateCandidateSelector();
    } catch (err) {
        showToast("Error loading candidates list", "error");
        console.error(err);
    }
}

function populateCandidateSelector() {
    const select = document.getElementById("candidate-select");
    select.innerHTML = '<option value="" disabled selected>-- Select Candidate --</option>';
    
    state.candidates.forEach(c => {
        const option = document.createElement("option");
        option.value = c.Id || c.id;
        option.textContent = `${c.FirstName || c.firstName} ${c.LastName || c.lastName} (${c.IsActive || c.isActive ? 'Active' : 'Inactive'})`;
        select.appendChild(option);
    });

    if (state.activeCandidateId) {
        select.value = state.activeCandidateId;
    }
}

async function registerCandidate(e) {
    e.preventDefault();
    const firstname = document.getElementById("cand-firstname").value.trim();
    const lastname = document.getElementById("cand-lastname").value.trim();
    const email = document.getElementById("cand-email").value.trim();
    const phone = document.getElementById("cand-phone").value.trim();

    try {
        const res = await fetch(`${API_BASE}/candidates`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ firstname, lastname, email, phonenumber: phone })
        });

        if (!res.ok) {
            const data = await res.json();
            throw new Error(data.message || "Failed to create candidate");
        }

        const candidate = await res.json();
        showToast("Candidate profile created successfully", "success");
        closeModal("register-candidate-modal");
        document.getElementById("register-candidate-form").reset();
        
        state.activeCandidateId = candidate.Id || candidate.id;
        await loadCandidates();
        onCandidateSelected();
    } catch (err) {
        showToast(err.message, "error");
        console.error(err);
    }
}

function onCandidateSelected() {
    const select = document.getElementById("candidate-select");
    state.activeCandidateId = parseInt(select.value);

    // Show resume selector and download candidate applications
    const resumeSection = document.getElementById("resume-section");
    resumeSection.classList.remove("hidden");
    
    loadCandidateResumes();
    loadCandidateApplications(state.activeCandidateId);
}

async function loadCandidateResumes() {
    if (!state.activeCandidateId) return;
    try {
        const res = await fetch(`${API_BASE}/candidates/${state.activeCandidateId}`);
        const candidate = await res.json();
        
        const resumeSelect = document.getElementById("resume-select");
        resumeSelect.innerHTML = '';
        
        const resumes = candidate.Resumes || candidate.resumes || [];
        if (resumes.length === 0) {
            resumeSelect.innerHTML = '<option value="" disabled selected>Upload a resume first...</option>';
        } else {
            resumes.forEach(r => {
                const option = document.createElement("option");
                option.value = r.Id || r.id;
                option.textContent = r.FileName || r.fileName;
                resumeSelect.appendChild(option);
            });
        }
    } catch (err) {
        console.error("Error loading resumes", err);
    }
}

async function uploadResume(e) {
    e.preventDefault();
    if (!state.activeCandidateId) {
        showToast("Please select a candidate first", "error");
        return;
    }

    const fileInput = document.getElementById("resume-file");
    if (fileInput.files.length === 0) return;

    const formData = new FormData();
    formData.append("file", fileInput.files[0]);

    try {
        const res = await fetch(`${API_BASE}/candidates/${state.activeCandidateId}/resume`, {
            method: 'POST',
            body: formData
        });

        if (!res.ok) throw new Error("Upload failed");

        showToast("Resume uploaded successfully", "success");
        closeModal("upload-resume-modal");
        document.getElementById("upload-resume-form").reset();
        await loadCandidateResumes();
    } catch (err) {
        showToast(err.message, "error");
        console.error(err);
    }
}

async function loadCandidateApplications(candId) {
    const listContainer = document.getElementById("candidate-apps-list");
    listContainer.innerHTML = '<div class="empty-state"><p>Loading application history...</p></div>';

    try {
        const res = await fetch(`${API_BASE}/jobapplications/candidate/${candId}`);
        const apps = await res.json();

        if (apps.length === 0) {
            listContainer.innerHTML = `
                <div class="empty-state">
                    <i data-lucide="info"></i>
                    <p>No applications submitted yet. Search jobs and apply!</p>
                </div>
            `;
        } else {
            listContainer.innerHTML = '';
            apps.forEach(app => {
                const job = app.JobListing || app.jobListing || {};
                const emp = job.Employer || job.employer || {};
                
                const item = document.createElement("div");
                item.className = "app-item";
                item.innerHTML = `
                    <div class="app-item-info">
                        <h4>${job.Title || job.title}</h4>
                        <p>${emp.CompanyName || emp.companyName || 'Unknown Company'}</p>
                        <div class="app-item-date">Applied on: ${formatDate(app.AppliedAt || app.appliedAt)}</div>
                    </div>
                    <span class="badge badge-status-${(app.ApplicationStatus || app.applicationStatus).toLowerCase()}">
                        ${app.ApplicationStatus || app.applicationStatus}
                    </span>
                `;
                listContainer.appendChild(item);
            });
        }
        lucide.createIcons();
    } catch (err) {
        console.error(err);
        listContainer.innerHTML = '<div class="empty-state"><p>Error loading applications history.</p></div>';
    }
}

// ==========================================
// EMPLOYERS LOGIC
// ==========================================
async function loadEmployers() {
    try {
        const res = await fetch(`${API_BASE}/employers`);
        state.employers = await res.json();
        populateEmployerSelector();
    } catch (err) {
        showToast("Error loading employers list", "error");
        console.error(err);
    }
}

function populateEmployerSelector() {
    const select = document.getElementById("employer-select");
    select.innerHTML = '<option value="" disabled selected>-- Select Employer --</option>';
    
    state.employers.forEach(e => {
        const option = document.createElement("option");
        option.value = e.Id || e.id;
        option.textContent = `${e.CompanyName || e.companyName} (${e.IsActive || e.isActive ? 'Active' : 'Inactive'})`;
        select.appendChild(option);
    });

    if (state.activeEmployerId) {
        select.value = state.activeEmployerId;
    }
}

async function registerEmployer(e) {
    e.preventDefault();
    const companyName = document.getElementById("emp-name").value.trim();
    const contactEmail = document.getElementById("emp-email").value.trim();
    const companyWebsite = document.getElementById("emp-website").value.trim();
    const industry = document.getElementById("emp-industry").value.trim();
    const location = document.getElementById("emp-location").value.trim();
    const description = document.getElementById("emp-desc").value.trim();

    try {
        const res = await fetch(`${API_BASE}/employers`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ companyName, contactEmail, companyWebsite, industry, location, description })
        });

        if (!res.ok) throw new Error("Failed to register employer profile");

        const employer = await res.json();
        showToast("Employer profile created successfully", "success");
        closeModal("register-employer-modal");
        document.getElementById("register-employer-form").reset();
        
        state.activeEmployerId = employer.Id || employer.id;
        await loadEmployers();
        onEmployerSelected();
    } catch (err) {
        showToast(err.message, "error");
        console.error(err);
    }
}

function onEmployerSelected() {
    const select = document.getElementById("employer-select");
    state.activeEmployerId = parseInt(select.value);

    // Show notifications and loaded jobs
    document.getElementById("employer-notifications-card").classList.remove("hidden");
    
    loadEmployerJobListings();
    loadEmployerNotifications(state.activeEmployerId);
}

async function loadEmployerJobListings() {
    if (!state.activeEmployerId) return;
    const container = document.getElementById("employer-listings");
    container.innerHTML = '<p>Loading your job listings...</p>';

    try {
        // We will filter by searching that employer's jobs using API or by local filter. Since our listing search API allows general search, let's fetch all jobs and filter locally, or we can add it. Let's filter locally for simplicity:
        const res = await fetch(`${API_BASE}/joblistings?status=All`);
        const listings = await res.json();
        const myListings = listings.filter(j => (j.EmployerId || j.employerId) === state.activeEmployerId);

        if (myListings.length === 0) {
            container.innerHTML = `
                <div class="empty-state">
                    <i data-lucide="info"></i>
                    <p>No job postings published yet. click 'Post a Job' to begin!</p>
                </div>
            `;
            document.getElementById("job-applicants-list").innerHTML = `
                <div class="empty-state">
                    <i data-lucide="users"></i>
                    <p>Select one of your job listings to view candidates who applied.</p>
                </div>
            `;
        } else {
            container.innerHTML = '';
            myListings.forEach(j => {
                const card = document.createElement("div");
                card.className = "employer-listing-card";
                card.id = `emp-job-${j.Id || j.id}`;
                card.onclick = () => selectJobForApplicants(j);
                
                const statusBadge = (j.Status || j.status).toLowerCase() === 'open' 
                    ? '<span class="badge badge-open">Open</span>' 
                    : '<span class="badge badge-closed">Closed</span>';

                card.innerHTML = `
                    <div>
                        <h4>${j.Title || j.title}</h4>
                        <div class="employer-listing-card-meta">
                            <span>Type: ${j.JobType || j.jobType}</span>
                            <span>Salary: ${j.SalaryRange || j.salaryRange}</span>
                            <span>Location: ${j.Location || j.location}</span>
                        </div>
                    </div>
                    <div>
                        ${statusBadge}
                    </div>
                `;
                container.appendChild(card);
            });

            // If we have selected one previously, re-select
            if (state.selectedJobForApplicants) {
                const found = myListings.find(j => (j.Id || j.id) === (state.selectedJobForApplicants.Id || state.selectedJobForApplicants.id));
                if (found) {
                    selectJobForApplicants(found);
                }
            }
        }
        lucide.createIcons();
    } catch (err) {
        console.error(err);
        container.innerHTML = '<p>Error loading listings.</p>';
    }
}

function selectJobForApplicants(job) {
    state.selectedJobForApplicants = job;
    
    // Highlight listing card
    document.querySelectorAll('.employer-listing-card').forEach(c => c.classList.remove('active'));
    const activeCard = document.getElementById(`emp-job-${job.Id || job.id}`);
    if (activeCard) activeCard.classList.add('active');

    loadJobApplicants(job.Id || job.id);
}

async function loadJobApplicants(jobId) {
    const listContainer = document.getElementById("job-applicants-list");
    listContainer.innerHTML = '<p>Loading applicants...</p>';

    try {
        const res = await fetch(`${API_BASE}/jobapplications/job/${jobId}`);
        const apps = await res.json();

        if (apps.length === 0) {
            listContainer.innerHTML = `
                <div class="empty-state">
                    <i data-lucide="user-x"></i>
                    <p>No applications received for this job listing yet.</p>
                </div>
            `;
        } else {
            listContainer.innerHTML = '';
            apps.forEach(app => {
                const cand = app.Candidate || app.candidate || {};
                const resume = app.Resume || app.resume || {};
                
                const card = document.createElement("div");
                card.className = "applicant-tracking-card";
                card.innerHTML = `
                    <div class="applicant-name">${cand.FirstName || cand.firstName} ${cand.LastName || cand.lastName}</div>
                    <div class="applicant-email">Email: ${cand.Email || cand.email}</div>
                    <div class="applicant-phone">Phone: ${cand.PhoneNumber || cand.phoneNumber || 'N/A'}</div>
                    
                    <a href="/${resume.FilePath || resume.filePath}" target="_blank" class="applicant-resume-link">
                        <i data-lucide="file-text"></i> View Resume (${resume.FileName || resume.fileName})
                    </a>

                    ${app.Notes || app.notes ? `<div class="applicant-notes">${app.Notes || app.notes}</div>` : ''}

                    <div class="status-update-row">
                        <select id="status-select-${app.Id || app.id}">
                            <option value="Applied" ${app.ApplicationStatus === 'Applied' ? 'selected' : ''}>Applied</option>
                            <option value="Reviewing" ${app.ApplicationStatus === 'Reviewing' ? 'selected' : ''}>Reviewing</option>
                            <option value="Interviewing" ${app.ApplicationStatus === 'Interviewing' ? 'selected' : ''}>Interviewing</option>
                            <option value="Offered" ${app.ApplicationStatus === 'Offered' ? 'selected' : ''}>Offered</option>
                            <option value="Rejected" ${app.ApplicationStatus === 'Rejected' ? 'selected' : ''}>Rejected</option>
                        </select>
                        <button class="btn btn-secondary btn-sm" onclick="updateApplicationStatus(${app.Id || app.id})">Update</button>
                    </div>
                `;
                listContainer.appendChild(card);
            });
        }
        lucide.createIcons();
    } catch (err) {
        console.error(err);
        listContainer.innerHTML = '<p>Error loading applicants.</p>';
    }
}

async function updateApplicationStatus(appId) {
    const select = document.getElementById(`status-select-${appId}`);
    const newStatus = select.value;

    try {
        const res = await fetch(`${API_BASE}/jobapplications/${appId}/status`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ status: newStatus, notes: "Status updated via portal" })
        });

        if (!res.ok) throw new Error("Failed to update status");

        showToast("Application status updated", "success");
        if (state.selectedJobForApplicants) {
            loadJobApplicants(state.selectedJobForApplicants.Id || state.selectedJobForApplicants.id);
        }
    } catch (err) {
        showToast(err.message, "error");
        console.error(err);
    }
}

async function postJobListing(e) {
    e.preventDefault();
    if (!state.activeEmployerId) {
        showToast("Please select acting employer profile first", "error");
        return;
    }

    const title = document.getElementById("job-title").value.trim();
    const location = document.getElementById("job-location").value.trim();
    const salaryRange = document.getElementById("job-salary").value.trim();
    const jobType = document.getElementById("job-type").value;
    const description = document.getElementById("job-desc").value.trim();
    const requirements = document.getElementById("job-reqs").value.trim();

    try {
        const res = await fetch(`${API_BASE}/joblistings`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                employerId: state.activeEmployerId,
                title,
                location,
                salaryRange,
                jobType,
                description,
                requirements,
                status: 'Open'
            })
        });

        if (!res.ok) throw new Error("Failed to publish job listing");

        showToast("Job listing published successfully", "success");
        closeModal("post-job-modal");
        document.getElementById("post-job-form").reset();
        
        loadEmployerJobListings();
    } catch (err) {
        showToast(err.message, "error");
        console.error(err);
    }
}

// ==========================================
// NOTIFICATIONS LOGIC
// ==========================================
async function loadEmployerNotifications(empId) {
    try {
        const res = await fetch(`${API_BASE}/employers/${empId}/notifications`);
        state.notifications = await res.json();
        
        const unreadCount = state.notifications.filter(n => !(n.IsRead || n.isRead)).length;
        
        const badge = document.getElementById("notification-badge");
        badge.textContent = unreadCount;
        if (unreadCount === 0) {
            badge.classList.add("hidden");
        } else {
            badge.classList.remove("hidden");
        }

        const list = document.getElementById("notifications-list");
        if (state.notifications.length === 0) {
            list.innerHTML = '<div class="p-2 text-center text-muted">No notifications</div>';
        } else {
            list.innerHTML = '';
            state.notifications.forEach(n => {
                const item = document.createElement("div");
                item.className = `notification-item ${n.IsRead || n.isRead ? 'read' : ''}`;
                item.innerHTML = `
                    <div>${n.Message || n.message}</div>
                    <span class="notification-time">${formatDate(n.CreatedAt || n.createdAt)}</span>
                `;
                list.appendChild(item);
            });
        }
    } catch (err) {
        console.error(err);
    }
}

function toggleNotificationsDropdown() {
    const dropdown = document.getElementById("notifications-dropdown");
    dropdown.classList.toggle("hidden");
}

async function markAllNotificationsAsRead() {
    if (!state.activeEmployerId) return;
    try {
        await fetch(`${API_BASE}/employers/${state.activeEmployerId}/notifications/read`, { method: 'PUT' });
        loadEmployerNotifications(state.activeEmployerId);
    } catch (err) {
        console.error(err);
    }
}

// ==========================================
// SEARCH & JOB LISTINGS
// ==========================================
async function loadJobListings() {
    const keyword = document.getElementById("search-keyword").value.trim();
    const location = document.getElementById("search-location").value.trim();
    const jobType = document.getElementById("search-jobtype").value;
    const status = document.getElementById("search-status").value;

    const listElement = document.getElementById("listings-list");
    listElement.innerHTML = '<p>Searching jobs...</p>';

    // Build query params
    const params = new URLSearchParams();
    if (keyword) params.append("search", keyword);
    if (location) params.append("location", location);
    if (jobType) params.append("jobType", jobType);
    if (status) params.append("status", status);

    try {
        const res = await fetch(`${API_BASE}/joblistings?${params.toString()}`);
        state.jobListings = await res.json();

        document.getElementById("job-count").textContent = state.jobListings.length;

        if (state.jobListings.length === 0) {
            listElement.innerHTML = `
                <div class="empty-state">
                    <i data-lucide="frown"></i>
                    <p>No job listings found matching your search criteria.</p>
                </div>
            `;
        } else {
            listElement.innerHTML = '';
            state.jobListings.forEach(j => {
                const emp = j.Employer || j.employer || {};
                
                const card = document.createElement("div");
                card.className = "job-card glass-card";
                card.onclick = () => viewJobDetails(j);

                card.innerHTML = `
                    <div class="job-card-left">
                        <span class="job-company">${emp.CompanyName || emp.companyName || 'Corporate'}</span>
                        <h4 class="job-title">${j.Title || j.title}</h4>
                        <div class="job-meta-row">
                            <span class="job-meta-item"><i data-lucide="map-pin"></i> ${j.Location || j.location}</span>
                            <span class="job-meta-item"><i data-lucide="banknote"></i> ${j.SalaryRange || j.salaryRange || 'Open'}</span>
                        </div>
                    </div>
                    <div>
                        <span class="badge badge-${(j.JobType || j.jobType).toLowerCase()}">${j.JobType || j.jobType}</span>
                    </div>
                `;
                listElement.appendChild(card);
            });
        }
        lucide.createIcons();
    } catch (err) {
        showToast("Failed to search listings", "error");
        console.error(err);
        listElement.innerHTML = '<p>Error loading job listings.</p>';
    }
}

function viewJobDetails(job) {
    state.selectedJobForDetails = job;
    
    const emp = job.Employer || job.employer || {};
    document.getElementById("job-details-title").textContent = job.Title || job.title;
    document.getElementById("job-details-company").textContent = emp.CompanyName || emp.companyName || 'Corporate';
    document.getElementById("job-details-type").textContent = job.JobType || job.jobType;
    document.getElementById("job-details-location").innerHTML = `<i data-lucide="map-pin"></i> ${job.Location || job.location}`;
    document.getElementById("job-details-salary").innerHTML = `<i data-lucide="banknote"></i> ${job.SalaryRange || job.salaryRange || 'Open'}`;
    document.getElementById("job-details-desc").textContent = job.Description || job.description;
    
    // Parse requirements text (split lines or bullet list)
    const reqsContainer = document.getElementById("job-details-reqs");
    reqsContainer.innerHTML = '';
    const reqs = job.Requirements || job.requirements || '';
    const lines = reqs.split('\n');
    const ul = document.createElement("ul");
    lines.forEach(l => {
        if (l.trim()) {
            const li = document.createElement("li");
            li.textContent = l.trim().startsWith('•') ? l.trim().substring(1).trim() : l.trim();
            ul.appendChild(li);
        }
    });
    reqsContainer.appendChild(ul);

    // Apply button state based on open/closed status
    const applySection = document.getElementById("apply-section");
    const closedMsg = document.getElementById("apply-closed-message");
    
    if ((job.Status || job.status).toLowerCase() === 'open') {
        applySection.classList.remove("hidden");
        closedMsg.classList.add("hidden");
    } else {
        applySection.classList.add("hidden");
        closedMsg.classList.remove("hidden");
    }

    openModal("job-details-modal");
    lucide.createIcons();
}

async function submitApplication() {
    if (!state.activeCandidateId) {
        showToast("Please register or select candidate profile first!", "error");
        return;
    }

    const resumeSelect = document.getElementById("resume-select");
    const resumeId = parseInt(resumeSelect.value);
    if (!resumeId) {
        showToast("Please upload a resume first!", "error");
        return;
    }

    const notes = document.getElementById("apply-notes").value.trim();
    const jobId = state.selectedJobForDetails.Id || state.selectedJobForDetails.id;

    try {
        const res = await fetch(`${API_BASE}/jobapplications`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                jobListingId: jobId,
                candidateId: state.activeCandidateId,
                resumeId: resumeId,
                notes: notes
            })
        });

        const data = await res.json();
        
        if (!res.ok) {
            throw new Error(data.message || "Failed to submit application");
        }

        showToast("Application submitted successfully!", "success");
        closeModal("job-details-modal");
        document.getElementById("apply-notes").value = '';
        
        // Reload history
        loadCandidateApplications(state.activeCandidateId);
    } catch (err) {
        showToast(err.message, "error");
        console.error(err);
    }
}

// ==========================================
// ADMIN DASHBOARD
// ==========================================
async function loadAdminData() {
    // 1. Fetch dashboard stats
    try {
        const res = await fetch(`${API_BASE}/dashboard/stats`);
        const stats = await res.json();

        document.getElementById("stat-total-jobs").textContent = stats.totalJobs;
        document.getElementById("stat-open-jobs").textContent = `${stats.openJobs} Open`;
        document.getElementById("stat-closed-jobs").textContent = `${stats.closedJobs} Closed`;
        document.getElementById("stat-total-apps").textContent = stats.totalApplications;
        document.getElementById("stat-total-employers").textContent = stats.totalEmployers;
        document.getElementById("stat-total-candidates").textContent = stats.totalCandidates;

        // Populate breakdowns
        renderDistributionBars("stat-status-distribution", stats.statusCounts, stats.totalApplications, "Status");
        renderDistributionBars("stat-type-distribution", stats.jobTypeCounts, stats.totalJobs, "JobType");
    } catch (err) {
        console.error("Stats fetch error", err);
    }

    // 2. Fetch specific subtab grid
    if (state.adminTab === 'candidates') {
        loadAdminCandidates();
    } else {
        loadAdminEmployers();
    }
}

function renderDistributionBars(elementId, dataArray, total, keyName) {
    const container = document.getElementById(elementId);
    container.innerHTML = '';
    
    if (dataArray.length === 0) {
        container.innerHTML = '<p class="text-muted text-center py-2">No data yet</p>';
        return;
    }

    dataArray.forEach(item => {
        const label = item[keyName.toLowerCase()] || item[keyName] || 'Other';
        const count = item.count || item.Count || 0;
        const percentage = total > 0 ? Math.round((count / total) * 100) : 0;

        const row = document.createElement("div");
        row.className = "dist-bar-row";
        row.innerHTML = `
            <div class="dist-bar-labels">
                <span class="dist-bar-label">${label}</span>
                <span class="dist-bar-val">${count} (${percentage}%)</span>
            </div>
            <div class="dist-progress-bg">
                <div class="dist-progress-fill" style="width: ${percentage}%"></div>
            </div>
        `;
        container.appendChild(row);
    });
}

async function loadAdminCandidates() {
    const tbody = document.getElementById("admin-candidates-tbody");
    tbody.innerHTML = '<tr><td colspan="6">Loading candidates...</td></tr>';

    try {
        const res = await fetch(`${API_BASE}/users/candidates`);
        const cands = await res.json();

        tbody.innerHTML = '';
        if (cands.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center">No candidates profiles registered</td></tr>';
            return;
        }

        cands.forEach(c => {
            const tr = document.createElement("tr");
            const statusLabel = c.IsActive || c.isActive 
                ? '<span class="text-success font-semibold">Active</span>' 
                : '<span class="text-danger font-semibold">Deactivated</span>';
                
            const actionBtnText = c.IsActive || c.isActive ? 'Deactivate' : 'Activate';
            const actionBtnClass = c.IsActive || c.isActive ? 'btn-secondary' : 'btn-primary';

            tr.innerHTML = `
                <td><strong>${c.FirstName || c.firstName} ${c.LastName || c.lastName}</strong></td>
                <td>${c.Email || c.email}</td>
                <td>${c.PhoneNumber || c.phoneNumber || 'N/A'}</td>
                <td>${formatDate(c.CreatedAt || c.createdAt)}</td>
                <td>${statusLabel}</td>
                <td>
                    <button class="btn ${actionBtnClass} btn-sm" onclick="toggleUserStatus('candidates', ${c.Id || c.id}, ${!(c.IsActive || c.isActive)})">
                        ${actionBtnText}
                    </button>
                </td>
            `;
            tbody.appendChild(tr);
        });
    } catch (err) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-danger">Error loading candidates</td></tr>';
    }
}

async function loadAdminEmployers() {
    const tbody = document.getElementById("admin-employers-tbody");
    tbody.innerHTML = '<tr><td colspan="6">Loading employers...</td></tr>';

    try {
        const res = await fetch(`${API_BASE}/users/employers`);
        const emps = await res.json();

        tbody.innerHTML = '';
        if (emps.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center">No employer profiles registered</td></tr>';
            return;
        }

        emps.forEach(e => {
            const tr = document.createElement("tr");
            const statusLabel = e.IsActive || e.isActive 
                ? '<span class="text-success font-semibold">Active</span>' 
                : '<span class="text-danger font-semibold">Deactivated</span>';
                
            const actionBtnText = e.IsActive || e.isActive ? 'Deactivate' : 'Activate';
            const actionBtnClass = e.IsActive || e.isActive ? 'btn-secondary' : 'btn-primary';

            tr.innerHTML = `
                <td><strong>${e.CompanyName || e.companyName}</strong></td>
                <td>${e.Industry || e.industry}</td>
                <td>${e.ContactEmail || e.contactEmail}</td>
                <td>${e.Location || e.location}</td>
                <td>${statusLabel}</td>
                <td>
                    <button class="btn ${actionBtnClass} btn-sm" onclick="toggleUserStatus('employers', ${e.Id || e.id}, ${!(e.IsActive || e.isActive)})">
                        ${actionBtnText}
                    </button>
                </td>
            `;
            tbody.appendChild(tr);
        });
    } catch (err) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-danger">Error loading employers</td></tr>';
    }
}

async function toggleUserStatus(type, userId, newStatus) {
    try {
        const res = await fetch(`${API_BASE}/users/${type}/${userId}/status`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ isActive: newStatus })
        });

        if (!res.ok) throw new Error("Status toggle failed");

        showToast("User status updated successfully", "success");
        loadAdminData();
        
        // Refresh dropdowns in portals
        if (type === 'candidates') {
            await loadCandidates();
        } else {
            await loadEmployers();
        }
    } catch (err) {
        showToast(err.message, "error");
    }
}

// ==========================================
// MODAL & GENERAL UTILITIES
// ==========================================
function openModal(modalId) {
    const overlay = document.getElementById(modalId);
    overlay.style.display = 'flex';
    setTimeout(() => {
        overlay.classList.add("active");
    }, 10);
}

function closeModal(modalId) {
    const overlay = document.getElementById(modalId);
    overlay.classList.remove("active");
    setTimeout(() => {
        overlay.style.display = 'none';
    }, 300);
}

function showToast(message, type = "info") {
    const container = document.getElementById("toast-container");
    const toast = document.createElement("div");
    toast.className = `toast toast-${type}`;
    
    let iconName = 'info';
    if (type === 'success') iconName = 'check-circle';
    if (type === 'error') iconName = 'alert-triangle';

    toast.innerHTML = `
        <i data-lucide="${iconName}" class="toast-icon toast-icon-${type}"></i>
        <span>${message}</span>
    `;
    container.appendChild(toast);
    lucide.createIcons();

    // Fade out and remove
    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateY(10px)';
        toast.style.transition = 'all 0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, 3500);
}

function formatDate(dateString) {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
}
