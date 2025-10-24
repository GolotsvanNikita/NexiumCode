import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import axios from 'axios';
import AceEditor from 'react-ace';

import 'ace-builds/src-noconflict/mode-csharp';
import 'ace-builds/src-noconflict/theme-monokai';
import 'ace-builds/src-noconflict/ext-language_tools';

import './Practice.css';

interface PracticeTask
{
    id: number;
    taskDescription: string;
    starterCode: string;
    testCases: string;
    averageTimeSeconds: number;
    expectedOutput: string;
}

interface Lesson
{
    id: number;
    title: string;
    practiceTasks?: PracticeTask[];
}

interface Course
{
    id: number;
    name: string;
    lessons: Lesson[];
    practiceProgress?: number;
}

interface PracticeProps
{
    userId: number | null;
}

export const Practice: React.FC<PracticeProps> = ({ userId }) =>
{
    const { courseId } = useParams<{ courseId: string }>();
    const [course, setCourse] = useState<Course | null>(null);
    const [selectedTask, setSelectedTask] = useState<PracticeTask | null>(null);
    const [code, setCode] = useState('');
    const [output, setOutput] = useState('');
    const [error, setError] = useState('');
    const [practiceProgress, setPracticeProgress] = useState(0);
    const [taskProgress, setTaskProgress] = useState<{ [key: number]: boolean }>({});
    const [loading, setLoading] = useState(true);
    const [isPracticeUnlocked, setIsPracticeUnlocked] = useState(false);

    useEffect(() =>
    {
        const fetchCourseAndProgress = async () =>
        {
            try
            {
                setLoading(true);

                const response = await axios.get(`http://localhost:5064/CSharpCourse.json?t=${Date.now()}`);
                setCourse(response.data);

                const progressRes = await axios.get(`/api/Progress/${courseId}?userId=${userId}`);
                const theoryProgress = progressRes.data.theoryProgress || 0;
                setIsPracticeUnlocked(theoryProgress === 100);

                const practiceProg = progressRes.data.practiceProgress || 0;
                setPracticeProgress(practiceProg);

                const progressKey = `course_${courseId}_user_${userId}_practice_progress`;
                const savedProgress = localStorage.getItem(progressKey);
                if (savedProgress)
                {
                    setTaskProgress(JSON.parse(savedProgress));
                }

                setLoading(false);
            }
            catch (err: any)
            {
                console.error('Error loading course or progress:', err);
                setError('Failed to load course or progress.');
                setLoading(false);
            }
        };
        if (userId && courseId)
        {
            fetchCourseAndProgress();
        }
    }, [courseId, userId]);

    useEffect(() =>
    {
        if (course && !selectedTask)
        {
            const firstTask = course.lessons.flatMap(lesson => lesson.practiceTasks || []).find(task => !taskProgress[task.id]);
            if (firstTask)
            {
                setSelectedTask(firstTask);
                setCode(firstTask.starterCode || '');
            }
            else
            {
                const allTasks = course.lessons.flatMap(lesson => lesson.practiceTasks || []);
                if (allTasks.length > 0)
                {
                    setSelectedTask(allTasks[0]);
                    setCode(allTasks[0].starterCode || '');
                }
            }
        }
    }, [course, selectedTask, taskProgress]);

    const selectTask = (task: PracticeTask) =>
    {
        setSelectedTask(task);
        setCode(task.starterCode || '');
        setOutput('');
        setError('');
    };

    const handleSubmit = async () =>
    {
        if (!selectedTask || !userId || !courseId)
        {
            setError('Missing task or user data.');
            return;
        }

        setOutput('');
        setError('');

        try
        {
            console.log('Submitting to:', `/api/PracticeTask/${selectedTask.id}/submit`);
            const response = await axios.post(`/api/PracticeTask/${selectedTask.id}/submit`,
            {
                userId,
                code
            });

            console.log('Full response:', response.data);

            if (response.data.message && response.data.message.includes('successfully'))
            {
                setOutput(response.data.output || 'Success!');
                alert('Task completed successfully!');

                const newTaskProgress = { ...taskProgress, [selectedTask.id]: true };
                setTaskProgress(newTaskProgress);
                const progressKey = `course_${courseId}_user_${userId}_practice_progress`;
                localStorage.setItem(progressKey, JSON.stringify(newTaskProgress));

                const progressRes = await axios.get(`/api/Progress/${courseId}?userId=${userId}`);
                setPracticeProgress(progressRes.data.practiceProgress || 0);
            }
            else
            {
                setError(response.data.message || 'Incorrect output or code error.');
            }
        }
        catch (err: any)
        {
            console.error('Submit error:', err.response?.data || err.message);
            if (err.response?.status === 404)
            {
                setError('Practice task endpoint not found. Check backend setup.');
            } else if (err.response?.status === 403)
            {
                setError('Practice locked. Complete theory first.');
            } else if (err.response?.status === 401)
            {
                setError('Unauthorized. Please log in again.');
            } else
            {
                setError(err.response?.data?.message || 'Error submitting code.');
            }
        }
    };

    if (loading) return <div className="loading">Loading...</div>;
    if (!course) return <div className="error">Course not found</div>;
    if (!isPracticeUnlocked) return <div className="error">Practice is locked. Complete theory first.</div>;

    const allTasks = course.lessons.flatMap(lesson => lesson.practiceTasks || []);

    return (
        <div className="practice-page">
            <aside className="practice-sidebar">
                <div className="sidebar-header">
                    <h2>{course.name} - Practice</h2>
                </div>

                <div className="progress-widget">
                    <div className="progress-label">Practice Progress</div>
                    <div className="progress-bar-container">
                        <div className="progress-bar-fill" style={{ width: `${practiceProgress}%` }}></div>
                    </div>
                    {practiceProgress === 100 && <div className="completion-badge">Completed!</div>}
                </div>

                <div className="tasks-nav">
                    <ul>
                        {allTasks.map(task => (
                            <li key={task.id}>
                                <button
                                    className={`task-btn ${selectedTask?.id === task.id ? 'active' : ''} ${taskProgress[task.id] ? 'completed' : ''}`}
                                    onClick={() => selectTask(task)}
                                >
                                    Task {task.id}: {task.taskDescription.substring(0, 30)}...
                                </button>
                            </li>
                        ))}
                    </ul>
                </div>
            </aside>

            <main className="practice-main">
                {selectedTask ? (
                    <div className="task-content">
                        <h1 className="task-title">{selectedTask.taskDescription}</h1>
                        <p className="task-description">Average completion time: {selectedTask.averageTimeSeconds} seconds.</p>

                        <div className="expected-output">
                            <h3>Expected Output</h3>
                            <pre>{selectedTask.expectedOutput || 'No expected output specified.'}</pre>
                        </div>

                        <AceEditor
                            mode="csharp"
                            theme="monokai"
                            value={code}
                            onChange={setCode}
                            name="code-editor"
                            editorProps={{ $blockScrolling: true }}
                            setOptions={{
                                enableBasicAutocompletion: true,
                                enableLiveAutocompletion: true,
                                enableSnippets: true,
                                showLineNumbers: true,
                                tabSize: 4,
                            }}
                            width="100%"
                            height="400px"
                            className="code-editor"
                        />

                        <button onClick={handleSubmit} className="submit-btn">Submit Code</button>

                        <div className="console">
                            <h3>Console Output</h3>
                            {output && <pre className="output">{output}</pre>}
                            {error && <pre className="error">{error}</pre>}
                        </div>
                    </div>
                ) : (
                    <div className="no-task">Select a practice task from the sidebar</div>
                )}
            </main>
        </div>
    );
};