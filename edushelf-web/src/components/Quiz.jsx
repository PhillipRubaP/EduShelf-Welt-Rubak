import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';

const Quiz = () => {
    const { documentId } = useParams();
    const [quiz, setQuiz] = useState(null);
    const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
    const [selectedAnswer, setSelectedAnswer] = useState(null);
    const [score, setScore] = useState(0);
    const [showScore, setShowScore] = useState(false);

    // Mock data for the quiz
    const mockQuiz = {
        id: 1,
        documentId: 1,
        questions: [
            {
                id: 1,
                text: 'What is 2 + 2?',
                answers: [
                    { id: 1, text: '3', isCorrect: false },
                    { id: 2, text: '4', isCorrect: true },
                    { id: 3, text: '5', isCorrect: false },
                ],
            },
            {
                id: 2,
                text: 'What is the capital of France?',
                answers: [
                    { id: 4, text: 'Berlin', isCorrect: false },
                    { id: 5, text: 'Madrid', isCorrect: false },
                    { id: 6, text: 'Paris', isCorrect: true },
                ],
            },
        ],
    };

    useEffect(() => {
        // In a real application, you would fetch the quiz from the API
        // For now, we'll use the mock data
        setQuiz(mockQuiz);
    }, [documentId]);

    const handleAnswerOptionClick = (isCorrect) => {
        if (isCorrect) {
            setScore(score + 1);
        }

        const nextQuestion = currentQuestionIndex + 1;
        if (nextQuestion < quiz.questions.length) {
            setCurrentQuestionIndex(nextQuestion);
        } else {
            setShowScore(true);
        }
    };

    if (showScore) {
        return (
            <div className="p-4">
                <h2 className="text-2xl font-bold mb-4">Quiz Completed</h2>
                <p className="text-lg">Your score is {score} out of {quiz.questions.length}</p>
            </div>
        );
    }

    if (!quiz) {
        return <div>Loading...</div>;
    }

    return (
        <div className="p-4">
            <h2 className="text-2xl font-bold mb-4">Quiz</h2>
            <div className="mb-4">
                <h3 className="text-xl">{quiz.questions[currentQuestionIndex].text}</h3>
            </div>
            <div className="grid grid-cols-1 gap-4">
                {quiz.questions[currentQuestionIndex].answers.map((answer) => (
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
};

export default Quiz;