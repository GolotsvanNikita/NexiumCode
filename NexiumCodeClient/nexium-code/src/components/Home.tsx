import { motion } from 'framer-motion';
import { Link } from 'react-router-dom';
import './Home.css';

export const Home: React.FC = () =>
{
    const fadeInVariants =
    {
        hidden: { opacity: 0, y: 50 },
        visible: { opacity: 1, y: 0, transition: { duration: 0.8, ease: 'easeOut' } },
    };

    const hoverVariants =
    {
        hover: { scale: 1.05, boxShadow: '0 6px 12px rgba(0, 0, 0, 0.4)', transition: { duration: 0.3 } },
    };

    return (
        <div className="home-container">
            <motion.section
                className="hero-section"
                initial="hidden"
                animate="visible"
                // @ts-ignore
                variants={fadeInVariants}
            >
                <motion.h1
                    // @ts-ignore
                    variants={fadeInVariants}
                    whileHover={{ scale: 1.05, color: '#ffffff' }}
                >
                    Welcome to NexiumCode!
                </motion.h1>
                <motion.p
                    // @ts-ignore
                    variants={fadeInVariants}
                    transition={{ delay: 0.2 }}
                >
                    Learn programming with practice problems, quizzes, and an active forum. Get started now!
                </motion.p>
                <motion.div
                    className="cta-buttons"
                    // @ts-ignore
                    variants={fadeInVariants}
                    transition={{ delay: 0.4 }}
                >
                    <Link to="/register" className="cta-button">Register</Link>
                    <Link to="/login" className="cta-button">Login</Link>
                </motion.div>
            </motion.section>

            <motion.section
                className="courses-section"
                initial="hidden"
                whileInView="visible"
                viewport={{ once: true }}
                // @ts-ignore
                variants={fadeInVariants}
            >
                <h2>Popular Courses</h2>
                <div className="courses-list">
                    <motion.div className="course-card" variants={hoverVariants} whileHover="hover">
                        <h3>Course 1: C# Basics</h3>
                        <p>Learn the fundamentals of C# programming, including syntax, variables, and control structures.</p>
                        <Link to="/courses/1">Go</Link>
                    </motion.div>
                </div>
            </motion.section>

            <motion.section
                className="forum-section"
                initial="hidden"
                whileInView="visible"
                viewport={{ once: true }}
                // @ts-ignore
                variants={fadeInVariants}
            >
                <h2>Community Forum</h2>
                <p>Discuss questions, share experiences, and find solutions with other students.</p>
                <Link to="/forum" className="cta-button">Go to the Forum</Link>
            </motion.section>

            <footer className="footer">
                <p>&copy; 2025 NexiumCode. All rights reserved.</p>
            </footer>
        </div>
    );
};