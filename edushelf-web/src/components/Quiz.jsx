import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getQuizzes, deleteQuiz } from '../services/api';
import QuizModal from './QuizModal';
import { FaPen, FaTrash, FaCheckCircle, FaTimesCircle } from 'react-icons/fa';
import './Files.css';
import './Quiz.css';

const Quiz = () => {
    const { quizTitle } = useParams();
    const navigate = useNavigate();
    const [quizzes, setQuizzes] = useState([]);
    const [selectedQuiz, setSelectedQuiz] = useState(null);
    const [editingQuiz, setEditingQuiz] = useState(null);
    const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
    const [score, setScore] = useState(0);
    const [showScore, setShowScore] = useState(false);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [openMenuId, setOpenMenuId] = useState(null);
    const [selectedAnswerId, setSelectedAnswerId] = useState(null);
    const [answerStatus, setAnswerStatus] = useState('');

    useEffect(() => {
        const fetchQuizzes = async () => {
            try {
                const data = await getQuizzes();
                setQuizzes(data);
            } catch (error) {
                console.error('Error fetching quizzes:', error);
            }
        };
        fetchQuizzes();
    }, []);

    useEffect(() => {
        if (quizTitle && quizzes.length > 0) {
            const quiz = quizzes.find((q) => q.title.toLowerCase().replace(/\s+/g, '-') === quizTitle);
            setSelectedQuiz(quiz);
        } else {
            setSelectedQuiz(null);
        }
    }, [quizTitle, quizzes]);

    useEffect(() => {
        if (selectedQuiz === null) {
            setShowScore(false);
        }
        if (quizTitle === undefined) {
            setSelectedQuiz(null);
            setCurrentQuestionIndex(0);
            setScore(0);
            setSelectedAnswerId(null);
            setAnswerStatus('');
        }
    }, [selectedQuiz, quizTitle]);

    const handleAnswerOptionClick = (answer) => {
        if (selectedAnswerId) return;

        setSelectedAnswerId(answer.id);
        const isCorrect = answer.isCorrect;
        setAnswerStatus(isCorrect ? 'correct' : 'incorrect');

        if (isCorrect) {
            setScore(score + 1);
        }

        setTimeout(() => {
            const nextQuestion = currentQuestionIndex + 1;
            if (nextQuestion < selectedQuiz.questions.length) {
                setCurrentQuestionIndex(nextQuestion);
                setSelectedAnswerId(null);
                setAnswerStatus('');
            } else {
                setShowScore(true);
            }
        }, 1000);
    };

    const handleQuizSaved = (savedQuiz) => {
        const existingQuiz = quizzes.find(q => q.id === savedQuiz.id);
        if (existingQuiz) {
            setQuizzes(quizzes.map(q => q.id === savedQuiz.id ? savedQuiz : q));
        } else {
            setQuizzes([...quizzes, savedQuiz]);
        }
    };

    const selectQuiz = (quiz) => {
        const quizTitleSlug = quiz.title.toLowerCase().replace(/\s+/g, '-');
        navigate(`/quiz/${quizTitleSlug}`);
    };

    const handleBackToQuizzes = () => {
        navigate('/quizzes');
    };

    const handleDelete = async (quizId) => {
        try {
            await deleteQuiz(quizId);
            setQuizzes(quizzes.filter(q => q.id !== quizId));
        } catch (error) {
            console.error('Error deleting quiz:', error);
        }
    };

    const handleEdit = (quiz) => {
        setEditingQuiz(quiz);
        setIsModalOpen(true);
    };

    const toggleMenu = (quizId) => {
        setOpenMenuId(openMenuId === quizId ? null : quizId);
    };

    if (showScore) {
        if (!selectedQuiz) {
            return null; // or a loading indicator
        }
        const percentage = Math.round((score / selectedQuiz.questions.length) * 100);
        const passed = percentage >= 50;
        return (
            <div className="quiz-results-container">
                <h2 className="quiz-results-title">Quiz Results</h2>
                {passed ? (
                    <FaCheckCircle className="quiz-results-icon success" />
                ) : (
                    <FaTimesCircle className="quiz-results-icon failure" />
                )}
                <p className="quiz-results-message">{passed ? "Nice job, you passed!" : "Sadly you didn't pass!"}</p>
                <div className="quiz-results-stats">
                    <div className="quiz-results-stat-card">
                        <h4>YOUR SCORE</h4>
                        <p>{percentage}%</p>
                        <span>PASSING SCORE: 50%</span>
                    </div>
                    <div className="quiz-results-stat-card">
                        <h4>YOUR POINTS</h4>
                        <p>{score}</p>
                        <span>PASSING POINTS: {Math.ceil(selectedQuiz.questions.length / 2)}</span>
                    </div>
                </div>
                <button onClick={handleBackToQuizzes} className="quiz-answer-btn">Back to Quizzes</button>
            </div>
        );
    }

    if (quizTitle && selectedQuiz) {
        return (
            <div className="quiz-container">
                <div className="quiz-question">
                    {selectedQuiz.questions[currentQuestionIndex].text}
                </div>
                <div className="quiz-answers">
                    {selectedQuiz.questions[currentQuestionIndex].answers.map((answer) => {
                        const isSelected = selectedAnswerId === answer.id;
                        const buttonClass = `quiz-answer-btn ${isSelected ? answerStatus : ''}`;
                        return (
                            <button
                                key={answer.id}
                                onClick={() => handleAnswerOptionClick(answer)}
                                className={buttonClass}
                                disabled={selectedAnswerId !== null}
                            >
                                {answer.text}
                            </button>
                        );
                    })}
                </div>
            </div>
        );
    }

    return (
        <div className="files-container">
            <div className="file-list">
                <div className="file-list-header">
                    <h2>Available Quizzes</h2>
                    <button onClick={() => setIsModalOpen(true)} className="add-file-button">+</button>
                </div>
                {isModalOpen && <QuizModal onClose={() => { setIsModalOpen(false); setEditingQuiz(null); }} onQuizSaved={handleQuizSaved} quiz={editingQuiz} />}
                <div className="file-grid">
                    {quizzes.map((quiz) => (
                        <div key={quiz.id} className="file-card quiz-card" onClick={() => selectQuiz(quiz)}>
                            <p>{quiz.title}</p>
                            <div className="file-card-buttons">
                                <button className="menu-button" onClick={(e) => { e.stopPropagation(); toggleMenu(quiz.id); }}>
                                    <div className="menu-icon"></div>
                                    <div className="menu-icon"></div>
                                    <div className="menu-icon"></div>
                                </button>
                                {openMenuId === quiz.id && (
                                    <div className="dropdown-menu">
                                        <button onClick={(e) => { e.stopPropagation(); handleEdit(quiz); }} title="Edit"><FaPen /></button>
                                        <button onClick={(e) => { e.stopPropagation(); handleDelete(quiz.id); }} title="Delete" className="delete-button"><FaTrash /></button>
                                    </div>
                                )}
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
};

export default Quiz;