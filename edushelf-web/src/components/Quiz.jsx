import React, { useState, useEffect } from 'react';
import { getQuizzes } from '../services/api';
import QuizModal from './QuizModal';

const Quiz = () => {
    const [quizzes, setQuizzes] = useState([]);
    const [selectedQuiz, setSelectedQuiz] = useState(null);
    const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
    const [score, setScore] = useState(0);
    const [showScore, setShowScore] = useState(false);
    const [isModalOpen, setIsModalOpen] = useState(false);
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

    const handleAnswerOptionClick = (isCorrect) => {
        if (isCorrect) {
            setScore(score + 1);
        }

        const nextQuestion = currentQuestionIndex + 1;
        if (nextQuestion < selectedQuiz.questions.length) {
            setCurrentQuestionIndex(nextQuestion);
        } else {
            setShowScore(true);
        }
    };

    const handleQuizCreated = (newQuiz) => {
        setQuizzes([...quizzes, newQuiz]);
    };

    const selectQuiz = (quiz) => {
        setSelectedQuiz(quiz);
        setCurrentQuestionIndex(0);
        setScore(0);
        setShowScore(false);
    };

    if (showScore) {
        return (
            <div className="p-4">
                <h2 className="text-2xl font-bold mb-4">Quiz Completed</h2>
                <p className="text-lg">Your score is {score} out of {selectedQuiz.questions.length}</p>
                <button onClick={() => setSelectedQuiz(null)}>Back to Quizzes</button>
            </div>
        );
    }

    if (selectedQuiz) {
        return (
            <div className="p-4">
                <h2 className="text-2xl font-bold mb-4">{selectedQuiz.title}</h2>
                <div className="mb-4">
                    <h3 className="text-xl">{selectedQuiz.questions[currentQuestionIndex].text}</h3>
                </div>
                <div className="grid grid-cols-1 gap-4">
                    {selectedQuiz.questions[currentQuestionIndex].answers.map((answer) => (
                        <button
                            key={answer.id}
                            onClick={() => handleAnswerOptionClick(answer.isCorrect)}
                            className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
                        >
                            {answer.text}
                        </button>
                    ))}
                </div>
            </div>
        );
    }

    return (
        <div className="p-4">
            <h2 className="text-2xl font-bold mb-4">Available Quizzes</h2>
            <button onClick={() => setIsModalOpen(true)}>Create Quiz</button>
            {isModalOpen && <QuizModal onClose={() => setIsModalOpen(false)} onQuizCreated={handleQuizCreated} />}
            <ul>
                {quizzes.map((quiz) => (
                    <li key={quiz.id} onClick={() => selectQuiz(quiz)} className="cursor-pointer hover:underline">
                        {quiz.title}
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default Quiz;