import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { useNavigate } from 'react-router-dom';

const Profile = () => {
    const [profile, setProfile] = useState({});
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [editing, setEditing] = useState(false);
    const [fullName, setFullName] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [profilePicture, setProfilePicture] = useState(null);
    const navigate = useNavigate();

    useEffect(() => {
        const token = localStorage.getItem('token');
        if (!token) {
            setError('You must be logged in to view this page.');
            setLoading(false);
            return;
        }

        api.get('/api/authe/profile', {
            headers: {
                Authorization: `Bearer ${token}`
            }
        })
            .then(res => {
                setProfile(res.data);
                setFullName(res.data.fullName);
                setLoading(false);
            })
            .catch(err => {
                setError("Failed to load profile.");
                setLoading(false);
            });
    }, []);

    const handleUpdate = async () => {
        try {
            const token = localStorage.getItem('token');
            const formData = new FormData();
            formData.append('fullName', fullName);
            if (newPassword) formData.append('newPassword', newPassword);
            if (profilePicture) formData.append('profilePicture', profilePicture);

            await api.put('/api/authe/profile', formData, {
                headers: {
                    Authorization: `Bearer ${token}`,
                    'Content-Type': 'multipart/form-data'
                }
            });

            window.location.reload(); // Reload to reflect updated profile
        } catch (err) {
            setError('Failed to update profile.');
        }
    };

    if (loading) return <div className="container mt-4">Loading...</div>;
    if (error) return <div className="container mt-4 text-danger">{error}</div>;

    return (
        <div className="container mt-4">
            <h3>Profile</h3>

            {profile.profilePictureUrl && (
                <img
                    src={profile.profilePictureUrl}
                    alt="Profile"
                    className="mb-3"
                    style={{ width: 150, height: 150, borderRadius: '50%', objectFit: 'cover' }}
                />
            )}

            {!editing ? (
                <>
                    <p><strong>Username:</strong> {profile.username}</p>
                    <p><strong>Full Name:</strong> {profile.fullName}</p>
                    <p><strong>Role:</strong> {profile.role}</p>

                    {profile.role === 'Administrator' && (
                        <div className="mt-4">
                            <button onClick={() => navigate('/admin/users')} className="btn btn-primary">
                                Manage Users
                            </button>
                        </div>
                    )}

                    <button onClick={() => setEditing(true)} className="btn btn-secondary mt-3">
                        Edit Profile
                    </button>
                </>
            ) : (
                <div className="mt-3">
                    <div className="mb-2">
                        <label>Full Name</label>
                        <input
                            className="form-control"
                            value={fullName}
                            onChange={e => setFullName(e.target.value)}
                        />
                    </div>

                    <div className="mb-2">
                        <label>New Password (leave blank to keep current)</label>
                        <input
                            type="password"
                            className="form-control"
                            value={newPassword}
                            onChange={e => setNewPassword(e.target.value)}
                        />
                    </div>

                    <div className="mb-3">
                        <label>Profile Picture</label>
                        <input
                            type="file"
                            className="form-control"
                            accept="image/*"
                            onChange={e => setProfilePicture(e.target.files[0])}
                        />
                    </div>

                    <button onClick={handleUpdate} className="btn btn-success me-2">Save</button>
                    <button onClick={() => setEditing(false)} className="btn btn-outline-secondary">Cancel</button>
                </div>
            )}
        </div>
    );
};

export default Profile;
