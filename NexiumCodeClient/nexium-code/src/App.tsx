import 'bootstrap/dist/css/bootstrap.min.css';
import './App.css';
import { Navigate, Route, Routes } from 'react-router-dom';
import { Home } from './components/Home.tsx';
import { Navbar } from './components/Navbar.tsx';
import {type JSX, useEffect, useState} from 'react';
import { Login } from './components/Login.tsx';
import { Register } from './components/Register.tsx';
import { Course } from './components/Course.tsx';
import { Forum } from './components/Forum.tsx';
import { ForumThread } from './components/ForumThread.tsx';
import { Lesson } from "./components/Lesson.tsx";
import { Profile } from "./components/Profile.tsx";
import {CustomCursor} from "./cursor/CustomCursor.tsx";
import { CreateTopic } from "./components/CreateTopic.tsx";
import axios from "axios";

const App: React.FC = () =>
{
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [userId, setUserId] = useState<number | null>(null);
    const [userAvatar, setUserAvatar] = useState<string>('/images/avatars/default-avatar.png');

    const PrivateRoute: React.FC<{ children: JSX.Element }> = ({ children }) =>
    {
        return isAuthenticated ? children : <Navigate to="/login" />;
    };

    const login = (userId: number) =>
    {
        setIsAuthenticated(true);
        setUserId(userId);
    };

    const logout = () =>
    {
        setIsAuthenticated(false);
        setUserId(null);
        setUserAvatar('/images/avatars/default-avatar.png');
    };

    const handleAvatarUpdate = (newAvatarUrl: string) => {
        setUserAvatar(newAvatarUrl);
    };

    useEffect(() =>
    {
        if (userId)
        {
            axios.get(`http://localhost:5064/api/user/${userId}`)
                .then(response => setUserAvatar(response.data.avatarUrl))
                .catch(err => console.error('Error loading avatar:', err));
        }
    }, [userId]);

    return (
        <>
            <CustomCursor />
            <Navbar isAuthenticated={isAuthenticated} logout={logout} userId={userId} userAvatar={userAvatar}/>
            <Routes>
                <Route path="/" element={<Home isAuthenticated={isAuthenticated} userId={userId} />} />
                <Route path="/login" element={<Login login={login} />} />
                <Route path="/register" element={<Register login={login} />} />

                <Route
                    path="/courses/:courseId"
                    element=
                        {
                            <PrivateRoute>
                                <Course userId={userId} />
                            </PrivateRoute>
                        }
                />

                <Route
                    path="/courses/:courseId/theory"
                    element=
                        {
                            <PrivateRoute>
                                <Lesson userId={userId} />
                            </PrivateRoute>
                        }
                />

                <Route
                    path="/profile/:userId"
                    element=
                        {
                            <PrivateRoute>
                                <Profile userId={userId} onAvatarUpdate={handleAvatarUpdate} />
                            </PrivateRoute>
                        }
                />

                <Route
                    path="/forum"
                    element={
                        <PrivateRoute>
                            <Forum />
                        </PrivateRoute>
                    }
                />

                <Route
                    path="/forum/:id"
                    element={
                        <PrivateRoute>
                            <ForumThread userId={userId} />
                        </PrivateRoute>
                    }
                />

                <Route
                    path="/forum/create"
                    element={
                        <PrivateRoute>
                            <CreateTopic userId={userId} />
                        </PrivateRoute>
                    }
                />
            </Routes>
        </>
    );
};

export default App;