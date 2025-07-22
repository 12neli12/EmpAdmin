import { useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

const Login = () => {
    const [form, setForm] = useState({ username: '', password: '' });
    const [error, setError] = useState('');
    const navigate = useNavigate(); // Use navigate here

    const login = async (e) => {
        e.preventDefault();
        try {
            const res = await axios.post('https://localhost:7013/api/auth/login', form);

            // Store token and user details in localStorage
            const { token, role, fullName, username } = res.data;
            localStorage.setItem('token', token);
            localStorage.setItem('user', JSON.stringify({ role, fullName, username }));

            // Redirect user to the dashboard after successful login
            navigate('/dashboard'); // Use navigate here for routing
        } catch (err) {
            setError('Login failed. Please check your credentials.');
        }
    };

    return (
        <div className="container mt-5">
            <h2>Login</h2>
            <form onSubmit={login}>
                <input
                    className="form-control mb-2"
                    placeholder="Username"
                    value={form.username}
                    onChange={(e) => setForm({ ...form, username: e.target.value })}
                />
                <input
                    className="form-control mb-2"
                    type="password"
                    placeholder="Password"
                    value={form.password}
                    onChange={(e) => setForm({ ...form, password: e.target.value })}
                />
                <button className="btn btn-primary">Login</button>
                {error && <div className="text-danger mt-2">{error}</div>}
            </form>
        </div>
    );
};

export default Login;
