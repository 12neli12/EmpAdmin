import React from 'react';
import { Navigate } from 'react-router-dom';

const PrivateRoute = ({ children, allowedRoles }) => {
    const token = localStorage.getItem('token');
    const user = JSON.parse(localStorage.getItem('user')); // Get user info from localStorage

    // If no token or user data exists, redirect to login
    if (!token || !user) {
        return <Navigate to="/login" />;
    }

    // If user doesn't have a valid role for the route, redirect to dashboard or home
    if (allowedRoles && !allowedRoles.includes(user?.role)) {
        return <Navigate to="/dashboard" />;
    }

    // If everything is okay, render the requested child component
    return children;
};

export default PrivateRoute;
