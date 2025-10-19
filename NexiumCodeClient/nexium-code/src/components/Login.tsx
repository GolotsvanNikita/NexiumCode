import {useState} from "react";
import {Link, useNavigate} from "react-router-dom";
import axios from "axios";
import './LogReg.css';

interface LoginProps
{
    login: (userId: number) => void;
}

export const Login: React.FC<LoginProps> = ({ login }) =>
{
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent) =>
    {
        e.preventDefault();
        setIsLoading(true);
        setError('');
        try
        {
            const response = await axios.post('http://localhost:5064/api/User/login', { email, password },
            {
                withCredentials: true,
            });
            login(response.data.userId);
            navigate('/');
        }
        catch (err: any)
        {
            setError(err.response?.data?.message || 'Login failed');
        }
        finally
        {
            setIsLoading(false);
        }
    };

    return (
        <div className="login-container">
            <h1>Login</h1>
            {error && <p className="error">{error}</p>}
            <form onSubmit={handleSubmit}>
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
                <button type="submit" disabled={isLoading}>
                    {isLoading ? 'Loading...' : 'Login'}
                </button>
            </form>
            <p>
                Don't have an account? <Link to="/register">Register</Link>
            </p>
        </div>
    );
}