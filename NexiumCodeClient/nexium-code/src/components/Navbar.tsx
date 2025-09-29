import { Link } from 'react-router-dom';
import { Dropdown } from 'react-bootstrap';
import './Navbar.css';

interface NavbarProps
{
    isAuthenticated: boolean;
    logout: () => void;
}

export const Navbar: React.FC<NavbarProps> = ({ isAuthenticated, logout }) =>
{
    return (
        <nav>
            <Link to="/">NexiumCode</Link>
            <div className="nav-center">
                <Dropdown>
                    <Dropdown.Toggle className="dropdown-toggle">Courses</Dropdown.Toggle>
                    <Dropdown.Menu className="dropdown-menu">
                        <Dropdown.Item as={Link} to={isAuthenticated ? '/courses/1' : '/login'} className="dropdown-item">
                            Course 1
                        </Dropdown.Item>
                        <Dropdown.Item as={Link} to={isAuthenticated ? '/courses/2' : '/login'} className="dropdown-item">
                            Course 2
                        </Dropdown.Item>
                    </Dropdown.Menu>
                </Dropdown>
                <Link to='/forum'>Forum</Link>
            </div>
            <div className="nav-right">
                {isAuthenticated ? (
                    <button onClick={logout}>Logout</button>
                ) : (
                    <>
                        <Link to="/login">Login</Link>
                        <Link to="/register">Register</Link>
                    </>
                )}
            </div>
        </nav>
    );
};