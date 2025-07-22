import React from 'react';
import { useNavigate } from 'react-router-dom';

const Dashboard = () => {
    const role = localStorage.getItem('role');
    const navigate = useNavigate();

    const handleLogout = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('role'); // Optional: clean up other user info
        navigate('/login');
    };

    return (
        <div className="container mt-4">
            <h3>Dashboard</h3>
            <p>Welcome, you're logged in as <strong>{role}</strong>.</p>

            <div className="mb-3">
                <a href="/profile">Profile</a> | <a href="/projects">Projects</a>
            </div>

            <button onClick={handleLogout} className="btn btn-danger">
                Logout
            </button>
        </div>
    );
};

export default Dashboard;
