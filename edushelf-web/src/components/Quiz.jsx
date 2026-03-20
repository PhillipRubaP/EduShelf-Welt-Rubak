import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getQuizzes, deleteQuiz } from '../services/api';
import QuizModal from './QuizModal';
import { FaPen, FaTrash, FaCheckCircle, FaTimesCircle } from 'react-icons/fa';
import './Files.css';
import './Quiz.css';

const PAGE_SIZE = 10;

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
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);

    const fetchQuizzes = async () => {
        try {
            const result = await getQuizzes(currentPage, PAGE_SIZE);
            if (result?.items) {
                setQuizzes(result.items);
                setTotalPages(result.totalPages);
            } else if (Array.isArray(result)) {
                setQuizzes(result);
            }
        } catch (error) {
            console.error('Failed to fetch quizzes', error);
        }
    };

    useEffect(() => {
        fetchQuizzes();
    }, [currentPage]);

    useEffect(() => {
        if (quizTitle && quizzes.length > 0) {
            const quiz = quizzes.find(q => q.title.toLowerCase().replace(/\s+/g, '-') === quizTitle);
            setSelectedQuiz(quiz);
        } else {
            setSelectedQuiz(null);
        }
    }, [quizTitle, quizzes]);

    useEffect(() => {
        if (!quizTitle) {
            setSelectedQuiz(null);
            setCurrentQuestionIndex(0);
            setScore(0);
            setSelectedAnswerId(null);
            setAnswerStatus('');
            setShowScore(false);
        }
    }, [quizTitle]);

    const handleAnswerOptionClick = (answer) => {
        if (selectedAnswerId) return;

        setSelectedAnswerId(answer.id);
        const isCorrect = answer.isCorrect;
        setAnswerStatus(isCorrect ? 'correct' : 'incorrect');

        if (isCorrect) setScore(score + 1);

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

    const handleQuizSaved = () => {
        setIsModalOpen(false);
        setEditingQuiz(null);
        fetchQuizzes();
    };

    const selectQuiz = (quiz) => {
        const slug = quiz.title.toLowerCase().replace(/\s+/g, '-');
        navigate(`/quiz/${slug}`);
    };

    const handleDelete = async (quizId) => {
        if (!window.confirm('Are you sure you want to delete this quiz?')) return;
        try {
            await deleteQuiz(quizId);
            fetchQuizzes();
        } catch (error) {
            console.error('Error deleting quiz:', error);
        }
    };

    const handleEdit = (quiz) => {
        setEditingQuiz(quiz);
        setIsModalOpen(true);
    };

    const toggleMenu = (quizId) => setOpenMenuId(openMenuId === quizId ? null : quizId);

    if (showScore && selectedQuiz) {
        const percentage = Math.round((score / selectedQuiz.questions.length) * 100);
        const passed = percentage >= 50;
        return (
            <div className="quiz-results-container">
                <h2 className="quiz-results-title">Quiz Results</h2>
                {passed
                    ? <FaCheckCircle className="quiz-results-icon success" />
                    : <FaTimesCircle className="quiz-results-icon failure" />
                }
                <p className="quiz-results-message">{passed ? 'Nice job, you passed!' : "Sadly you didn't pass!"}</p>
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
                <button onClick={() => navigate('/quizzes')} className="quiz-answer-btn">Back to Quizzes</button>
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
                    {selectedQuiz.questions[currentQuestionIndex].answers.map((answer) => (
                        <button
                            key={answer.id}
                            onClick={() => handleAnswerOptionClick(answer)}
                            className={`quiz-answer-btn ${selectedAnswerId === answer.id ? answerStatus : ''}`}
                            disabled={selectedAnswerId !== null}
                        >
                            {answer.text}
                        </button>
                    ))}
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

                {isModalOpen && (
                    <QuizModal
                        onClose={() => { setIsModalOpen(false); setEditingQuiz(null); }}
                        onQuizSaved={handleQuizSaved}
                        quiz={editingQuiz}
                    />
                )}

                <div className="file-grid">
                    {quizzes.map((quiz) => (
                        <div
                            key={quiz.id}
                            className="file-card quiz-card"
                            style={{ zIndex: openMenuId === quiz.id ? 100 : 1 }}
                            onClick={() => selectQuiz(quiz)}
                        >
                            <p>{quiz.title}</p>
                            <div className="file-card-buttons">
                                <button
                                    className="menu-button"
                                    onClick={(e) => { e.stopPropagation(); toggleMenu(quiz.id); }}
                                >
                                    <div className="menu-icon" />
                                    <div className="menu-icon" />
                                    <div className="menu-icon" />
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

                {totalPages > 1 && (
                    <div className="pagination-controls">
                        <button
                            onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
                            disabled={currentPage === 1}
                            className="pagination-button"
                        >
                            Previous
                        </button>
                        <span className="pagination-info">Page {currentPage} of {totalPages}</span>
                        <button
                            onClick={() => setCurrentPage(prev => Math.min(prev + 1, totalPages))}
                            disabled={currentPage === totalPages}
                            className="pagination-button"
                        >
                            Next
                        </button>
                    </div>
                )}
            </div>
        </div>
    );
};

export default Quiz;