import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';
import './Course.css';

interface Course
{
    id: number;
    name: string;
    description: string;
}

interface CourseLandingProps
{
    userId: number | null;
}

export const Course: React.FC<CourseLandingProps> = ({ userId }) =>
{
    const { courseId } = useParams<{ courseId: string }>();
    const navigate = useNavigate();
    const [course, setCourse] = useState<Course | null>(null);
    const [theoryProgress, setTheoryProgress] = useState(0);
    const [isPracticeUnlocked, setIsPracticeUnlocked] = useState(false);
    const [loading, setLoading] = useState(true);

    useEffect(() =>
    {
        const fetchCourse = async () =>
        {
            try
            {
                const response = await axios.get(`http://localhost:5064/CSharpCourse.json`);
                setCourse(response.data);

                const progressKey = `course_${courseId}_user_${userId}_progress`;
                const savedProgress = localStorage.getItem(progressKey);

                if (savedProgress)
                {
                    const progress = JSON.parse(savedProgress);
                    setTheoryProgress(progress.theoryProgress || 0);
                    setIsPracticeUnlocked(progress.theoryProgress === 100);
                }
                else
                {
                    try
                    {
                        const progressRes = await axios.get(`/api/Progress/${courseId}?userId=${userId}`);
                        const serverProgress = progressRes.data.theoryProgress || 0;
                        setTheoryProgress(serverProgress);
                        setIsPracticeUnlocked(serverProgress === 100);

                        localStorage.setItem(progressKey, JSON.stringify({
                            lessonProgress: {},
                            theoryProgress: serverProgress
                        }));
                    }
                    catch (err)
                    {
                        console.log('No progress found on server');
                        setTheoryProgress(0);
                        setIsPracticeUnlocked(false);
                    }
                }

                setLoading(false);
            }
            catch (err)
            {
                console.error('Error loading course:', err);
                setLoading(false);
            }
        };
        fetchCourse();
    }, [courseId, userId]);

    if (loading) return <div className="loading">Loading...</div>;
    if (!course) return <div className="error">Course not found</div>;

    return (
        <div className="course-landing">
            <div className="landing-container">
                <h1 className="course-title">{course.name}</h1>

                <p className="course-description">{course.description}</p>

                <div className="progress-section">
                    <h3>Your Progress</h3>
                    <div className="progress-bar">
                        <div className="progress-fill" style={{ width: `${theoryProgress}%` }}>
                            {theoryProgress > 10 && `${theoryProgress}%`}
                        </div>
                    </div>
                    <p className="progress-text">
                        Theory: {theoryProgress}% completed
                    </p>
                </div>

                <div className="choice-buttons">
                    <button
                        className="choice-btn theory-btn"
                        onClick={() => navigate(`/courses/${courseId}/theory`)}
                    >
                        <span className="btn-icon">THEORY</span>
                    </button>

                    <button
                        className={`choice-btn practice-btn ${!isPracticeUnlocked ? 'locked' : ''}`}
                        onClick={() => isPracticeUnlocked && navigate(`/courses/${courseId}/practice`)}
                        disabled={!isPracticeUnlocked}
                    >
                        <span className="btn-icon">PRACTICE</span>
                        {!isPracticeUnlocked && <div className="lock-text">Complete theory to unlock</div>}
                    </button>
                </div>
            </div>
        </div>
    );
};