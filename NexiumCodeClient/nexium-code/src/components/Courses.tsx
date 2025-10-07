import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { motion } from 'framer-motion';
import axios from 'axios';
import './Courses.css';

interface QuizQuestion {
    id: number;
    questionText: string;
    options: string;
    correctAnswer?: string;
}

interface PracticeTask {
    id: number;
    taskDescription: string;
    starterCode: string;
    testCases: string;
    averageTimeSeconds: number;
}

interface Lesson {
    id: number;
    title: string;
    content: string | null;
    isTheory: boolean;
    order: number;
    quizQuestions?: QuizQuestion[];
    practiceTasks?: PracticeTask[];
}

interface Course {
    id: number;
    name: string;
    description: string;
    lessons: Lesson[];
}

interface CoursesProps {
    userId: number | null;
}

export const Courses: React.FC<CoursesProps> = ({ userId }) => {
    const { courseId } = useParams<{ courseId: string }>();
    const [course, setCourse] = useState<Course | null>(null);
    const [selectedLesson, setSelectedLesson] = useState<Lesson | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    const fadeInVariants = {
        hidden: { opacity: 0, y: 20 },
        visible: { opacity: 1, y: 0, transition: { duration: 0.6, ease: 'easeOut' } },
    };

    const hoverVariants = {
        hover: { scale: 1.05, boxShadow: '0 6px 12px rgba(0, 0, 0, 0.4)', transition: { duration: 0.3 } },
    };

    useEffect(() => {
        const fetchCourse = async () => {
            if (!userId || !courseId) {
                setError('User ID or Course ID is missing');
                setLoading(false);
                return;
            }

            try {
                const response = await axios.get(`http://localhost:5064/%D0%A1SharpCourse.json`);
                const courseData = response.data;
                if (courseId === '1' && courseData.id === 1) {
                    setCourse(courseData);
                } else {
                    setError('Course not found or ID mismatch');
                }
                setLoading(false);
            } catch (err: any) {
                setError(err.response?.data?.message || 'Failed to load course');
                setLoading(false);
            }
        };
        fetchCourse();
    }, [courseId, userId]);

    const fetchLessonDetails = async (lessonId: number) => {
        if (!userId || !courseId) {
            setError('User ID or Course ID is missing');
            return;
        }

        if (course) {
            const lesson = course.lessons.find(l => l.id === lessonId);
            if (lesson) {
                setSelectedLesson(lesson);
            } else {
                setError('Lesson not found');
            }
        }
    };

    if (loading) return <div className="loading">Loading...</div>;
    if (error) return <div className="error">{error}</div>;
    if (!course) return <div className="error">Course not found</div>;

    return (
        <div className="course-container">
            <motion.section
                className="course-header"
                initial="hidden"
                animate="visible"
                // @ts-ignore
                variants={fadeInVariants}
            >
                <h1>{course.name}</h1>
                <p>{course.description}</p>
            </motion.section>

            <motion.section
                className="lessons-section"
                initial="hidden"
                whileInView="visible"
                viewport={{ once: true }}
                // @ts-ignore
                variants={fadeInVariants}
            >
                <h2>Lessons</h2>
                <div className="lessons-list">
                    {course.lessons && course.lessons.length > 0 ? (
                        course.lessons.map((lesson) => (
                            <motion.div
                                key={lesson.id}
                                className="lesson-card"
                                variants={hoverVariants}
                                whileHover="hover"
                            >
                                <h3>{lesson.title}</h3>
                                <button
                                    className="cta-button"
                                    onClick={() => fetchLessonDetails(lesson.id)}
                                >
                                    Start Lesson
                                </button>
                            </motion.div>
                        ))
                    ) : (
                        <p>No lessons available</p>
                    )}
                </div>
            </motion.section>

            {selectedLesson && (
                <motion.section
                    className="lesson-details"
                    initial="hidden"
                    animate="visible"
                    // @ts-ignore
                    variants={fadeInVariants}
                >
                    <h2>{selectedLesson.title}</h2>
                    <p>{selectedLesson.content || 'No content available'}</p>
                    {selectedLesson.quizQuestions && selectedLesson.quizQuestions.length > 0 && (
                        <div className="quiz-questions">
                            <h4>Quiz Questions:</h4>
                            {selectedLesson.quizQuestions.map((question) => (
                                <div key={question.id} className="quiz-question">
                                    <p>{question.questionText}</p>
                                    {/* Добавь логику для тестов позже */}
                                </div>
                            ))}
                        </div>
                    )}
                    {selectedLesson.practiceTasks && selectedLesson.practiceTasks.length > 0 && (
                        <div className="practice-tasks">
                            <h4>Practice Tasks:</h4>
                            {selectedLesson.practiceTasks.map((task) => (
                                <div key={task.id} className="practice-task">
                                    <p>{task.taskDescription}</p>
                                </div>
                            ))}
                        </div>
                    )}
                    <button className="cta-button" onClick={() => setSelectedLesson(null)}>
                        Close
                    </button>
                </motion.section>
            )}
        </div>
    );
};