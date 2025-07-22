import { useEffect, useState } from 'react';
import api from '../services/api';

const Projects = () => {
    const [user, setUser] = useState(null);
    const [projects, setProjects] = useState([]);
    const [employees, setEmployees] = useState([]);

    const [newProjectName, setNewProjectName] = useState('');
    const [newProjectDescription, setNewProjectDescription] = useState('');
    const [selectedEmployeeIds, setSelectedEmployeeIds] = useState([]);

    const [newTaskTitles, setNewTaskTitles] = useState({});
    const [newTaskAssignees, setNewTaskAssignees] = useState({});

    const [editingTask, setEditingTask] = useState(null);
    const [editingProject, setEditingProject] = useState(null);
    const [editedTaskTitle, setEditedTaskTitle] = useState('');
    const [editedTaskAssignee, setEditedTaskAssignee] = useState('');
    const [editedProjectName, setEditedProjectName] = useState('');
    const [editedProjectDesc, setEditedProjectDesc] = useState('');

    useEffect(() => {
        const fetchData = async () => {
            try {
                const profileRes = await api.get('api/authe/profile');
                setUser(profileRes.data);

                const projectsRes = await api.get('api/project');
                setProjects(projectsRes.data);

                if (profileRes.data.role === 'Administrator') {
                    const employeesRes = await api.get('api/authe/employees');
                    setEmployees(employeesRes.data);
                } else {
                    // For employees, gather teammates from their projects
                    const teammates = new Map();
                    projectsRes.data.forEach(p => {
                        p.assignedEmployees?.forEach(e => teammates.set(e.id, e));
                    });
                    setEmployees(Array.from(teammates.values()));
                }
            } catch (err) {
                console.error('Error loading data:', err);
                alert('Failed to load data from server.');
            }
        };
        fetchData();
    }, []);

    const isPartOfProject = (project) => project.assignedEmployees?.some(e => e.id === user.id);

    const toggleEmployee = (id) => {
        setSelectedEmployeeIds(prev =>
            prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]
        );
    };

    const refreshProjects = async () => {
        const updated = await api.get('api/project');
        setProjects(updated.data);
    };

    const createProject = async () => {
        try {
            await api.post('/api/project', {
                name: newProjectName,
                description: newProjectDescription,
                employeeIds: selectedEmployeeIds
            });
            await refreshProjects();
            setNewProjectName('');
            setNewProjectDescription('');
            setSelectedEmployeeIds([]);
        } catch (err) {
            console.error('Project creation failed:', err);
            alert(err.response?.data?.message || 'Error creating project');
        }
    };

    const createTask = async (projectId) => {
        const title = newTaskTitles[projectId];
        const assigneeId = newTaskAssignees[projectId];
        if (!title || !assigneeId) {
            alert('Please fill task name and select assignee');
            return;
        }
        try {
            await api.post('/api/task', {
                title,
                projectId,
                assignedToId: assigneeId
            });
            await refreshProjects();
            setNewTaskTitles(prev => ({ ...prev, [projectId]: '' }));
            setNewTaskAssignees(prev => ({ ...prev, [projectId]: '' }));
        } catch (err) {
            console.error('Task creation failed:', err);
            alert(err.response?.data?.message || 'Error creating task');
        }
    };

    const markTaskCompleted = async (taskId) => {
        try {
            await api.put(`/api/task/${taskId}/complete`);
            await refreshProjects();
        } catch (err) {
            console.error('Failed to mark task complete:', err);
            alert('Could not mark task as completed');
        }
    };

    const deleteProject = async (projectId) => {
        try {
            await api.delete(`/api/project/${projectId}`);
            setProjects(prev => prev.filter(p => p.id !== projectId));
        } catch (err) {
            alert(err.response?.data?.message || 'Failed to delete project');
        }
    };

    const startEditTask = (task) => {
        const project = projects.find(p => p.tasks?.some(t => t.id === task.id));
        setEditingTask({ ...task, projectId: project?.id });
        setEditedTaskTitle(task.title);
        setEditedTaskAssignee(task.assignedToId);
    };


    const saveTaskEdit = async () => {
        try {
            await api.put(`/api/task/${editingTask.id}`, {
                title: editedTaskTitle,
                assignedToId: editedTaskAssignee,
                description: editingTask.description || ''
            });
            setEditingTask(null);
            await refreshProjects();
        } catch (err) {
            alert('Error updating task');
        }
    };

    const deleteTask = async (taskId) => {
        if (!window.confirm("Delete this task?")) return;
        try {
            await api.delete(`/api/task/${taskId}`);
            await refreshProjects();
        } catch (err) {
            alert('Could not delete task');
        }
    };

    const startEditProject = (project) => {
        setEditingProject(project);
        setEditedProjectName(project.name);
        setEditedProjectDesc(project.description);
    };

    const saveProjectEdit = async () => {
        try {
            await api.put(`/api/project/${editingProject.id}`, {
                name: editedProjectName,
                description: editedProjectDesc
            });
            setEditingProject(null);
            await refreshProjects();
        } catch (err) {
            alert('Error updating project');
        }
    };

    const getEmployeeName = (id) => {
        const emp = employees.find(e => e.id === id);
        return emp ? emp.fullName : 'Unknown';
    };

    if (!user) return <div className="text-center mt-5">Loading...</div>;

    return (
        <div className="container mt-4">
            {user.role === 'Administrator' && (
                <div className="card p-3 mb-4 shadow-sm">
                    <h5>Create New Project</h5>
                    <input className="form-control mb-2" placeholder="Project name" value={newProjectName} onChange={(e) => setNewProjectName(e.target.value)} />
                    <input className="form-control mb-2" placeholder="Project description" value={newProjectDescription} onChange={(e) => setNewProjectDescription(e.target.value)} />
                    <label>Select Employees:</label>
                    <div className="d-flex flex-wrap mb-2">
                        {employees.map(emp => (
                            <div key={emp.id} className="form-check me-3">
                                <input className="form-check-input" type="checkbox" id={`emp-${emp.id}`} checked={selectedEmployeeIds.includes(emp.id)} onChange={() => toggleEmployee(emp.id)} />
                                <label className="form-check-label" htmlFor={`emp-${emp.id}`}>{emp.fullName}</label>
                            </div>
                        ))}
                    </div>
                    <button className="btn btn-primary" onClick={createProject}>Create Project</button>
                </div>
            )}

            { projects.map(project => {
                const isEmployeeAssignedToProject = project.assignedEmployees?.some(emp => emp.id === user.id);

                return (
                    <div key={project.id} className="accordion-item mb-2">
                        <h2 className="accordion-header" id={`heading-${project.id}`}>
                            <button className="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target={`#collapse-${project.id}`} aria-expanded="false">
                                <strong>{project.name}</strong> <small className="text-muted ms-2">({project.description})</small>
                            </button>
                        </h2>
                        <div id={`collapse-${project.id}`} className="accordion-collapse collapse" data-bs-parent="#projectsAccordion">
                            <div className="accordion-body">
                                {user.role === 'Administrator' && (
                                    <>
                                        <button className="btn btn-sm btn-danger mb-2" onClick={() => deleteProject(project.id)}>Delete Project</button>
                                        <button className="btn btn-sm btn-warning mb-2 ms-2" onClick={() => startEditProject(project)}>Edit Project</button>
                                    </>
                                )}

                                <ul className="list-group mb-3">
                                    {project.tasks?.length > 0 ? (
                                        project.tasks.map(task => {
                                            console.log('Task:', task);
                                            console.log('User ID:', user.id, 'Assigned To:', task.assignedToId);
                                            console.log('Condition:',
                                                !task.isCompleted,
                                                parseInt(task.assignedToId) === parseInt(user.id),
                                                user.role === 'Administrator'
                                            );

                                            return (
                                                <li key={task.id} className="list-group-item d-flex justify-content-between align-items-center">
                                                    <div>
                                                        <strong>{task.title}</strong>
                                                        {task.isCompleted && <span className="badge bg-success ms-2">Completed</span>}
                                                        <br />
                                                        <span className="badge bg-secondary mt-1">
                                                            Assigned to: {getEmployeeName(task.assignedToId)}
                                                        </span>
                                                    </div>
                                                    <div className="d-flex gap-2">
                                                        {!task.isCompleted &&
                                                            (parseInt(task.assignedToId) === parseInt(user.id) || user.role === 'Administrator') && (
                                                                <button
                                                                    className="btn btn-sm btn-outline-success"
                                                                    onClick={() => markTaskCompleted(task.id)}
                                                                >
                                                                    ✅ Complete
                                                                </button>
                                                            )}
                                                            {(parseInt(task.assignedToId) === parseInt(user.id) || user.role === 'Administrator') && (
                                                                <button
                                                                    className="btn btn-sm btn-outline-primary"
                                                                    onClick={() => startEditTask(task)}
                                                                >
                                                                    ✏️ Edit
                                                                </button>
                                                             )}
                                                    </div>
                                                </li>
                                            );
                                        })
                                    ) : (
                                        <li className="list-group-item text-muted">No tasks yet</li>
                                    )}

                                </ul>

                               
                                    <div className="border p-3 rounded bg-light">
                                        <input
                                            className="form-control mb-2"
                                            type="text"
                                            placeholder="New task name"
                                            value={newTaskTitles[project.id] || ''}
                                            onChange={(e) => setNewTaskTitles(prev => ({ ...prev, [project.id]: e.target.value }))}
                                        />
                                        <select
                                            className="form-select mb-2"
                                            value={newTaskAssignees[project.id] || ''}
                                            onChange={(e) => setNewTaskAssignees(prev => ({ ...prev, [project.id]: e.target.value }))}
                                        >
                                            <option value="">Select assignee</option>
                                            {project.assignedEmployees?.map(e => (
                                                <option key={e.id} value={e.id}>{e.fullName}</option>
                                            ))}
                                        </select>
                                        <button className="btn btn-success btn-sm" onClick={() => createTask(project.id)}>Add Task</button>
                                    </div>
                                
                            </div>
                        </div>
                    </div>
                );
            })}


            {editingTask && (
                <div className="card p-3 my-3 shadow-sm">
                    <h5>Edit Task</h5>
                    <input className="form-control mb-2" value={editedTaskTitle} onChange={(e) => setEditedTaskTitle(e.target.value)} />
                    <select
                        className="form-select mb-2"
                        value={editedTaskAssignee}
                        onChange={(e) => setEditedTaskAssignee(e.target.value)}
                    >
                        <option value="">Select Assignee</option>
                        {projects.find(p => p.id === editingTask.projectId)?.assignedEmployees?.map(e => (
                            <option key={e.id} value={e.id}>{e.fullName}</option>
                        ))}
                    </select>
                    <div className="d-flex gap-2">
                        <button className="btn btn-success" onClick={saveTaskEdit}>Save</button>
                        <button className="btn btn-secondary" onClick={() => setEditingTask(null)}>Cancel</button>
                    </div>
                </div>
            )}


            {editingProject && (
                <div className="card p-3 my-3 shadow-sm">
                    <h5>Edit Project</h5>
                    <input className="form-control mb-2" value={editedProjectName} onChange={(e) => setEditedProjectName(e.target.value)} />
                    <input className="form-control mb-2" value={editedProjectDesc} onChange={(e) => setEditedProjectDesc(e.target.value)} />
                    <div className="d-flex gap-2">
                        <button className="btn btn-success" onClick={saveProjectEdit}>Save</button>
                        <button className="btn btn-secondary" onClick={() => setEditingProject(null)}>Cancel</button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default Projects;