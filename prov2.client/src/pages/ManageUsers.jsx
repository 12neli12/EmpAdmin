import React, { useEffect, useState } from 'react';
import api from '../services/api';

const initialFormState = {
    id: null,
    username: '',
    fullName: '',
    password: '',
    role: 'User',
};

const ManageUsers = () => {
    const [users, setUsers] = useState([]);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(true);
    const [form, setForm] = useState(initialFormState);
    const [showForm, setShowForm] = useState(false);
    const [isEditing, setIsEditing] = useState(false);

    useEffect(() => {
        fetchUsers();
    }, []);

    const fetchUsers = () => {
        const token = localStorage.getItem('token');
        if (!token) {
            setError('You must be logged in to manage users.');
            setLoading(false);
            return;
        }

        api.get('/api/authe/employees', {
            headers: { Authorization: `Bearer ${token}` }
        })
            .then(res => {
                setUsers(res.data);
                setLoading(false);
            })
            .catch(err => {
                console.error('Error fetching users:', err.response?.data || err.message);
                setError("Could not fetch users.");
                setLoading(false);
            });
    };

    const handleChange = (e) => {
        const { name, value } = e.target;
        setForm(prev => ({ ...prev, [name]: value }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        const token = localStorage.getItem('token');
        if (!token) {
            setError('You must be logged in.');
            return;
        }

        try {
            if (isEditing) {
                await api.put(`/api/authe/employees/${form.id}`, form, {
                    headers: {
                        Authorization: `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    }
                });
            } else {
                await api.post('/api/authe/create', form, {
                    headers: {
                        Authorization: `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    }
                });
            }

            setForm(initialFormState);
            setShowForm(false);
            setIsEditing(false);
            fetchUsers();
        } catch (err) {
            console.error("Error saving user:", err.response?.data || err.message);
            alert("Error: " + (err.response?.data || err.message));
        }
    };

    const handleDelete = async (id) => {
        const token = localStorage.getItem('token');
        if (!token) return;

        try {
            await api.delete(`/api/authe/employees/${id}`, {
                headers: { Authorization: `Bearer ${token}` }
            });

            setUsers(users.filter(u => u.id !== id));
        } catch (err) {
            const msg = err.response?.data || 'Failed to delete user.';
            if (typeof msg === 'string' && msg.includes('assigned')) {
                alert("This user is assigned to tasks and cannot be deleted.");
            } else {
                alert("Failed to delete user: " + msg);
            }
            console.error('Delete failed:', msg);
        }
    };


    const handleEdit = (user) => {
        setForm({ ...user, password: '' }); // Leave password blank unless changing
        setIsEditing(true);
        setShowForm(true);
    };

    const handleCreate = () => {
        setForm(initialFormState);
        setIsEditing(false);
        setShowForm(true);
    };

    if (loading) return <div>Loading...</div>;

    return (
        <div className="container mt-4">
            <h3>User Management</h3>
            {error && <div className="text-danger mb-3">{error}</div>}

            <button className="btn btn-primary mb-3" onClick={handleCreate}>
                {showForm && !isEditing ? 'Cancel' : 'Create New User'}
            </button>

            {showForm && (
                <form onSubmit={handleSubmit} className="mb-4">
                    <div className="form-group mb-2">
                        <label>Username</label>
                        <input
                            type="text"
                            name="username"
                            value={form.username}
                            onChange={handleChange}
                            className="form-control"
                            required
                        />
                    </div>
                    <div className="form-group mb-2">
                        <label>Full Name</label>
                        <input
                            type="text"
                            name="fullName"
                            value={form.fullName}
                            onChange={handleChange}
                            className="form-control"
                            required
                        />
                    </div>
                    <div className="form-group mb-2">
                        <label>{isEditing ? 'New Password (optional)' : 'Password'}</label>
                        <input
                            type="password"
                            name="password"
                            value={form.password}
                            onChange={handleChange}
                            className="form-control"
                            required={!isEditing}
                        />
                    </div>
                    <div className="form-group mb-2">
                        <label>Role</label>
                        <select
                            name="role"
                            value={form.role}
                            onChange={handleChange}
                            className="form-control"
                        >
                            <option value="User">User</option>
                            <option value="Administrator">Administrator</option>
                        </select>
                    </div>
                    <button type="submit" className="btn btn-success mt-2">
                        {isEditing ? 'Update User' : 'Create User'}
                    </button>
                </form>
            )}

            {users.length === 0 ? (
                <p>No users found.</p>
            ) : (
                <table className="table table-bordered">
                    <thead>
                        <tr>
                            <th>Username</th>
                            <th>Full Name</th>
                            <th>Role</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {users.map(user => (
                            <tr key={user.id}>
                                <td>{user.username}</td>
                                <td>{user.fullName}</td>
                                <td>{user.role}</td>
                                <td>
                                    <button
                                        className="btn btn-sm btn-warning me-2"
                                        onClick={() => handleEdit(user)}
                                    >
                                        Edit
                                    </button>
                                    <button
                                        className="btn btn-sm btn-danger"
                                        onClick={() => handleDelete(user.id)}
                                    >
                                        Delete
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    );
};

export default ManageUsers;
