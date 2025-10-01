import {useState} from "react";
import {Link, useNavigate} from "react-router-dom";
import axios from "axios";
import './Register.css';

interface RegisterProps
{
    login: (userId: number) => void;
}

export const Register: React.FC<RegisterProps> = ({login}) =>
{
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent) =>
    {
        e.preventDefault();
        setError('');
        setIsLoading(true);

        if (password !== confirmPassword)
        {
            setError('Passwords do not match');
            setIsLoading(false);
            return;
        }
        if (password.length < 6)
        {
            setError('Password must be at least 6 characters');
            setIsLoading(false);
            return;
        }

        try
        {
            const response = await axios.post('http://localhost:5064/api/User/register', { username, email, password },
            {
                withCredentials: true,
            });
            login(response.data.userId);
            navigate('/');
        }
        catch (err: any)
        {
            setError(err.response?.data?.message || 'Registration failed');
        }
        finally
        {
            setIsLoading(false);
        }
    };

    return(
        <div className="register-container">
            <h1>Register</h1>
            {error && <p className="error">{error}</p>}
            <form onSubmit={handleSubmit}>
                <div className="form-group">
                    <label htmlFor="username">Username</label>
                    <input
                        type="text"
                        id="username"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        required
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="email">Email</label>
                    <input
                        type="email"
                        id="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="password">Password</label>
                    <input
                        type="password"
                        id="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="confirmPassword">Confirm Password</label>
                    <input
                        type="password"
                        id="confirmPassword"
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        required
                    />
                </div>
                <button type="submit" disabled={isLoading}>
                    {isLoading ? 'Loading...' : 'Register'}
                </button>
            </form>
            <p>
                Already have an account? <Link to="/login">Login</Link>
            </p>
        </div>
    )
}