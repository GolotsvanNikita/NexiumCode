import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import axios from 'axios';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { vscDarkPlus } from 'react-syntax-highlighter/dist/esm/styles/prism';
import './Lesson.css';

interface QuizQuestion
{
    id: number;
    questionText: string;
    options: string;
    correctAnswer?: string;
    explanation?: string;
}

interface Lesson
{
    id: number;
    title: string;
    content: string | null;
    isTheory: boolean;
    order: number;
    codeExamples?: CodeExample[];
    starterCode?: string;
    quizQuestions?: QuizQuestion[];
}

interface CodeExample
{
    language: string;
    code: string;
}

interface Course
{
    id: number;
    name: string;
    description: string;
    lessons: Lesson[];
}

interface LessonProps
{
    userId: number | null;
}

interface QuizResult
{
    questionId: number;
    isCorrect: boolean;
    correctAnswer: string;
    explanation?: string;
}

export const Lesson: React.FC<LessonProps> = ({ userId }) =>
{
    const { courseId } = useParams<{ courseId: string }>();
    const [course, setCourse] = useState<Course | null>(null);
    const [selectedLesson, setSelectedLesson] = useState<Lesson | null>(null);
    const [quizAnswers, setQuizAnswers] = useState<{ [key: number]: string }>({});
    const [quizResults, setQuizResults] = useState<{ [key: number]: QuizResult }>({});
    const [showResults, setShowResults] = useState(false);
    const [lessonProgress, setLessonProgress] = useState<{ [key: number]: boolean }>({});
    const [theoryProgress, setTheoryProgress] = useState(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() =>
    {
        const fetchCourse = async () =>
        {
            try
            {
                setLoading(true);
                setError(null);

                const response = await axios.get(`http://localhost:5064/CSharpCourse.json?t=${Date.now()}`);
                console.log('Course data received:', response.data);

                if (!response.data)
                {
                    throw new Error('No course data received');
                }

                if (!response.data.lessons || !Array.isArray(response.data.lessons))
                {
                    throw new Error('Invalid course data structure');
                }

                setCourse(response.data);

                const theoryLessons = response.data.lessons.filter((l: Lesson) => l.isTheory === true);
                console.log('Theory lessons found:', theoryLessons.length);
                console.log('Theory lessons:', theoryLessons);

                if (theoryLessons.length === 0)
                {
                    throw new Error('No theory lessons found in course');
                }

                if (theoryLessons.length > 0)
                {
                    setSelectedLesson(theoryLessons[0]);
                    console.log('Selected first lesson:', theoryLessons[0]);
                }

                const progressKey = `course_${courseId}_user_${userId}_progress`;
                const savedProgress = localStorage.getItem(progressKey);
                console.log('Progress key:', progressKey);
                console.log('Saved progress:', savedProgress);

                if (savedProgress)
                {
                    const progress = JSON.parse(savedProgress);
                    setLessonProgress(progress.lessonProgress || {});
                    setTheoryProgress(progress.theoryProgress || 0);
                }
                else
                {
                    setLessonProgress({});
                    setTheoryProgress(0);
                }

                setLoading(false);
            }
            catch (err: any)
            {
                console.error('Error loading course:', err);
                setError(err.message || 'Failed to load course');
                setLoading(false);
            }
        };
        fetchCourse();
    }, [courseId, userId]);

    const handleQuizAnswer = (questionId: number, answer: string) =>
    {
        setQuizAnswers(prev => ({ ...prev, [questionId]: answer }));
    };

    const submitTest = async () =>
    {
        if (!selectedLesson?.quizQuestions || !course) return;

        const results: { [key: number]: QuizResult } = {};
        let correctCount = 0;

        for (const question of selectedLesson.quizQuestions)
        {
            try
            {
                const response = await axios.post(`/api/QuizQuestion/${question.id}/submit`,
                {
                    answer: quizAnswers[question.id]
                });

                results[question.id] =
                {
                    questionId: question.id,
                    isCorrect: response.data.isCorrect,
                    correctAnswer: response.data.correctAnswer,
                    explanation: response.data.explanation
                };

                if (response.data.isCorrect) correctCount++;
            }
            catch (err)
            {
                console.error('Error submitting answer:', err);
            }
        }

        setQuizResults(results);
        const score = (correctCount / selectedLesson.quizQuestions.length) * 100;
        console.log('Quiz score:', score);

        if (score >= 70 && !lessonProgress[selectedLesson.id])
        {
            const newLessonProgress = { ...lessonProgress, [selectedLesson.id]: true };
            setLessonProgress(newLessonProgress);

            const theoryLessons = course.lessons.filter(l => l.isTheory);
            const completedLessons = Object.keys(newLessonProgress).length;
            const newTheoryProgress = Math.round((completedLessons / theoryLessons.length) * 100);
            console.log('Completed lessons:', completedLessons, 'of', theoryLessons.length);
            console.log('New theory progress:', newTheoryProgress);

            setTheoryProgress(newTheoryProgress);

            const progressKey = `course_${courseId}_user_${userId}_progress`;
            localStorage.setItem(progressKey, JSON.stringify({
                lessonProgress: newLessonProgress,
                theoryProgress: newTheoryProgress
            }));

            try
            {
                await axios.post(`/api/Progress/${courseId}/theory`,
                {
                    userId,
                    progress: newTheoryProgress,
                });
                console.log('Progress updated on server successfully');
            }
            catch (err: any)
            {
                console.error('Error updating progress on server:', err);
                console.error('Error details:', err.response?.data);
            }
        }

        setShowResults(true);
    };

    const retryTest = () =>
    {
        setQuizAnswers({});
        setQuizResults({});
        setShowResults(false);
    };

    const selectLesson = (lesson: Lesson) =>
    {
        setSelectedLesson(lesson);
        setShowResults(false);
        setQuizAnswers({});
        setQuizResults({});
    };

    if (loading) return <div className="loading">Loading...</div>;
    if (error) return <div className="error">Error: {error}</div>;
    if (!course) return <div className="error">Course not found</div>;
    if (!course.lessons) return <div className="error">No lessons in course</div>;

    const theoryLessons = course.lessons.filter(l => l.isTheory);
    const allLessonsCompleted = theoryLessons.length > 0 && theoryLessons.every(lesson => lessonProgress[lesson.id]);

    if (theoryLessons.length === 0)
    {
        return <div className="error">No theory lessons found</div>;
    }

    console.log('Rendering - Total theory lessons:', theoryLessons.length);
    console.log('Lesson progress:', lessonProgress);
    console.log('All lessons completed:', allLessonsCompleted);

    console.log('course:', course);
    console.log('theoryLessons:', theoryLessons);
    console.log('theoryLessons type:', Array.isArray(theoryLessons));
    console.log('theoryLessons length:', theoryLessons?.length);
    console.log('selectedLesson:', selectedLesson);

    return (
        <div className="lesson-page">
            <aside className="lesson-sidebar">
                <div className="sidebar-header">
                    <h2>Theory</h2>
                </div>

                <div className="progress-widget">
                    <div className="progress-label">
                        Overall Progress: {theoryProgress}%
                    </div>
                    <div className="progress-bar-container">
                        <div
                            className="progress-bar-fill"
                            style={{ width: `${theoryProgress}%` }}
                        />
                    </div>
                    {allLessonsCompleted && (
                        <div className="completion-badge">
                            Theory Completed!
                        </div>
                    )}
                </div>

                <div className="lessons-nav">
                    {theoryLessons.map((lesson, index) =>
                    {
                        console.log('Rendering lesson item:', index, lesson);
                        return (
                            <div
                                key={lesson.id}
                                onClick={() =>
                                {
                                    console.log('Clicked lesson:', lesson.id);
                                    selectLesson(lesson);
                                }}
                                className='lesson-nav-item'
                            >
                                <div>Lesson {index + 1}: {lesson.title}</div>
                                {lessonProgress[lesson.id] && <div>COMPLETED</div>}
                            </div>
                        );
                    })}
                </div>
            </aside>

            <main className="lesson-content">
                {selectedLesson ? (
                    <div className="lesson-content-wrapper">
                        <h1 className="lesson-heading">{selectedLesson.title}</h1>

                        <div className="lesson-text">
                            {selectedLesson.content}
                        </div>

                        {selectedLesson.codeExamples && selectedLesson.codeExamples.length > 0 && (
                            <div className="code-examples-section">
                                {selectedLesson.codeExamples.map((example, index) => (
                                    <div key={index} className="code-example-wrapper">
                                        <div className="code-header">
                                            <span className="code-language">{example.language}</span>
                                        </div>
                                        <SyntaxHighlighter
                                            language={example.language}
                                            style={vscDarkPlus}
                                            customStyle={{
                                                margin: 0,
                                                borderRadius: '0 0 12px 12px',
                                                padding: '24px'
                                            }}
                                        >
                                            {example.code}
                                        </SyntaxHighlighter>
                                    </div>
                                ))}
                            </div>
                        )}

                        {selectedLesson.quizQuestions && selectedLesson.quizQuestions.length > 0 && (
                            <div className="quiz-section">
                                <h3 className="quiz-heading">Quiz</h3>

                                {selectedLesson.quizQuestions.map((question, index) => (
                                    <div key={question.id} className="quiz-question-card">
                                        <p className="question-text">
                                            {index + 1}. {question.questionText}
                                        </p>

                                        <div className="options-list">
                                            {question.options.split('\n').map((option, i) => (
                                                <label
                                                    key={i}
                                                    className={`option-label ${
                                                        quizAnswers[question.id] === option.charAt(0) ? 'selected' : ''
                                                    } ${
                                                        showResults && quizResults[question.id]
                                                            ? quizResults[question.id].isCorrect && quizAnswers[question.id] === option.charAt(0)
                                                                ? 'correct'
                                                                : !quizResults[question.id].isCorrect && quizAnswers[question.id] === option.charAt(0)
                                                                    ? 'incorrect'
                                                                    : quizResults[question.id].correctAnswer === option.charAt(0)
                                                                        ? 'correct-answer'
                                                                        : ''
                                                            : ''
                                                    }`}
                                                >
                                                    <input
                                                        type="radio"
                                                        name={`q${question.id}`}
                                                        value={option.charAt(0)}
                                                        checked={quizAnswers[question.id] === option.charAt(0)}
                                                        onChange={(e) => handleQuizAnswer(question.id, e.target.value)}
                                                        disabled={showResults}
                                                    />
                                                    <span>{option}</span>
                                                </label>
                                            ))}
                                        </div>

                                        {showResults && quizResults[question.id] && (
                                            <div className={`result-message ${quizResults[question.id].isCorrect ? 'correct-msg' : 'incorrect-msg'}`}>
                                                {quizResults[question.id].isCorrect ? (
                                                    <p>Correct!</p>
                                                ) : (
                                                    <>
                                                        <p>Incorrect. Correct answer: {quizResults[question.id].correctAnswer}</p>
                                                        {quizResults[question.id].explanation && (
                                                            <p className="explanation">
                                                                <strong>Explanation:</strong> {quizResults[question.id].explanation}
                                                            </p>
                                                        )}
                                                    </>
                                                )}
                                            </div>
                                        )}
                                    </div>
                                ))}

                                <div className="quiz-actions">
                                    {!showResults ? (
                                        <button
                                            onClick={submitTest}
                                            className="submit-btn"
                                            disabled={Object.keys(quizAnswers).length !== selectedLesson.quizQuestions.length}
                                        >
                                            Submit Answers
                                        </button>
                                    ) : (
                                        <div className="results-actions">
                                            {lessonProgress[selectedLesson.id] ? (
                                                <div className="success-message">
                                                    <h4>Lesson Completed!</h4>
                                                    <p>You can proceed to the next lesson.</p>
                                                </div>
                                            ) : (
                                                <div className="retry-message">
                                                    <p>Score below 70%. Try again!</p>
                                                    <button onClick={retryTest} className="retry-btn">
                                                        Retry Quiz
                                                    </button>
                                                </div>
                                            )}
                                        </div>
                                    )}
                                </div>
                            </div>
                        )}
                    </div>
                ) : (
                    <div className="error">No lesson selected</div>
                )}
            </main>
        </div>
    );
};