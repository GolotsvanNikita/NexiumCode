import 'bootstrap/dist/css/bootstrap.min.css';
import './App.css';
import {Navigate, Route, Routes} from "react-router-dom";
import {Home} from "./components/Home.tsx";
import {Navbar} from "./components/Navbar.tsx";
import {type JSX, useState} from "react";
import {Login} from "./components/Login.tsx";
import {Register} from "./components/Register.tsx";
import {Courses} from "./components/Courses.tsx";
import {Forum} from "./components/Forum.tsx";

const App: React.FC = () =>
{
    const [isAuthenticated, setIsAuthenticated] = useState(false)
    const [userId, setUserId] = useState<number | null>(null);

    const PrivateRoute: React.FC<{children: JSX.Element}> = ({children}) =>
    {
        return isAuthenticated ? children : <Navigate to='/login' />
    }

    const login = (userId: number) =>
    {
        setIsAuthenticated(true);
        setUserId(userId);
    };

    const logout = () =>
    {
        setIsAuthenticated(false);
        setUserId(null);
    };

    return(
        <>
            <Navbar isAuthenticated={isAuthenticated} logout={logout} userId={userId} />
            <Routes>
                <Route path="/" element={<Home />} />
                <Route path="/login" element={<Login login={login} />} />
                <Route path="/register" element={<Register login={login} />} />
                <Route path='/courses' element={<PrivateRoute><Courses /></PrivateRoute>} />
                <Route path="/forum" element={<PrivateRoute><Forum /></PrivateRoute>} />
            </Routes>
        </>
    )
}

export default App
