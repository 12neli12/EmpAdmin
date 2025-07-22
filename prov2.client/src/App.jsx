import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Profile from './pages/Profile';
import Projects from './pages/Projects';
import ManageUsers from './pages/ManageUsers';
import PrivateRoute from './components/PrivateRoute';

function App() {
    return (
        <Router>
            <Routes>
                <Route path="/login" element={<Login />} />
                <Route path="/" element={<PrivateRoute><Dashboard /></PrivateRoute>} />
                <Route path="/dashboard" element={<PrivateRoute><Dashboard /></PrivateRoute>} />
                <Route path="/profile" element={<PrivateRoute><Profile /></PrivateRoute>} />
                <Route path="/projects" element={<PrivateRoute><Projects /></PrivateRoute>} />
                <Route
                    path="/admin/users"
                    element={
                        <PrivateRoute allowedRoles={['Administrator']}>
                            <ManageUsers />
                        </PrivateRoute>
                    }
                />
            </Routes>
        </Router>
    );
}

export default App;
