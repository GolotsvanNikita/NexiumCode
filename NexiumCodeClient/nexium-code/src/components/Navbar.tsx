import { Link } from 'react-router-dom';
import { Dropdown } from 'react-bootstrap';
import './Navbar.css';

interface NavbarProps
{
    isAuthenticated: boolean;
    logout: () => void;
    userId: number | null;
}

export const Navbar: React.FC<NavbarProps> = ({ isAuthenticated, logout, userId }) =>
{
    return (
        <nav>
            <Link to="/">NexiumCode</Link>
            <div className="nav-center">
                <Dropdown>
                    <Dropdown.Toggle className="dropdown-toggle">Courses</Dropdown.Toggle>
                    <Dropdown.Menu className="dropdown-menu">
                        <Dropdown.Item as={Link} to={isAuthenticated ? '/courses/1' : '/login'} className="dropdown-item">
                            <div className="course-item">
                                <img
                                    src="http://localhost:5064/images/C_sharp.png"
                                    alt="C#"
                                    className="course-icon"
                                />
                                <span>C# Basics</span>
                            </div>
                        </Dropdown.Item>
                        <Dropdown.Item as={Link} to={isAuthenticated ? '/courses/2' : '/login'} className="dropdown-item">
                            <div className="course-item">
                                <span>Course 2</span>
                            </div>
                        </Dropdown.Item>
                    </Dropdown.Menu>
                </Dropdown>
                <Link to='/forum'>Forum</Link>
            </div>
            <div className="nav-right">
                {isAuthenticated ? (
                    <>
                        <Link to={`/profile/${userId}`}>Profile</Link>
                        <button onClick={logout}>Logout</button>
                    </>
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