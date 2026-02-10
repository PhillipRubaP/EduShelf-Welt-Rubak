import React, { useState, useEffect } from 'react';
import './QuizModal.css';
import { createQuiz, updateQuiz } from '../services/api';

const QuizModal = ({ onClose, onQuizSaved, quiz }) => {
    const [title, setTitle] = useState('');
    const [questions, setQuestions] = useState([{ text: '', answers: [{ text: '', isCorrect: false }] }]);

    useEffect(() => {
        if (quiz) {
            setTitle(quiz.title);
            setQuestions(quiz.questions);
        }
    }, [quiz]);

    const handleAddQuestion = () => {
        setQuestions([...questions, { text: '', answers: [{ text: '', isCorrect: false }] }]);
    };

    const handleDeleteQuestion = (questionIndex) => {
        const newQuestions = questions.filter((_, index) => index !== questionIndex);
        setQuestions(newQuestions);
    };

    const handleAddAnswer = (questionIndex) => {
        const newQuestions = [...questions];
        newQuestions[questionIndex].answers.push({ text: '', isCorrect: false });
        setQuestions(newQuestions);
    };

    const handleDeleteAnswer = (questionIndex, answerIndex) => {
        const newQuestions = [...questions];
        newQuestions[questionIndex].answers = newQuestions[questionIndex].answers.filter((_, index) => index !== answerIndex);
        setQuestions(newQuestions);
    };

    const handleQuestionChange = (index, event) => {
        const newQuestions = [...questions];
        newQuestions[index].text = event.target.value;
        setQuestions(newQuestions);
    };

    const handleAnswerChange = (questionIndex, answerIndex, event) => {
        const newQuestions = [...questions];
        newQuestions[questionIndex].answers[answerIndex].text = event.target.value;
        setQuestions(newQuestions);
    };

    const handleCorrectAnswerChange = (questionIndex, answerIndex) => {
        const newQuestions = [...questions];
        newQuestions[questionIndex].answers.forEach((answer, index) => {
            answer.isCorrect = index === answerIndex;
        });
        setQuestions(newQuestions);
    };

    const handleSubmit = async (event) => {
        event.preventDefault();
        const quizData = {
            title,
            questions,
        };
        try {
            if (quiz) {
                const updatedQuiz = await updateQuiz(quiz.id, quizData);
                onQuizSaved(updatedQuiz);
            } else {
                const newQuiz = await createQuiz({ title: quizData.title, questions: quizData.questions });
                onQuizSaved(newQuiz);
            }
            onClose();
        } catch (error) {
            console.error('Error saving quiz:', error);
        }
    };

    return (
        <div className="modal-overlay">
            <div className="modal-container modal-xl">
                <button className="modal-close-button" onClick={onClose}>
                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                </button>

                <div className="modal-header">
                    <h2 className="modal-title">{quiz ? 'Edit Quiz' : 'Create Quiz'}</h2>
                </div>

                <div className="modal-body">
                    <form onSubmit={handleSubmit} id="quiz-form">
                        <div className="form-group">
                            <label className="form-label">Quiz Title</label>
                            <input
                                type="text"
                                value={title}
                                onChange={(e) => setTitle(e.target.value)}
                                className="form-input"
                                placeholder="Enter a descriptive title"
                                required
                            />
                        </div>

                        <div className="quiz-questions-list">
                            {questions.map((question, qIndex) => (
                                <div key={qIndex} className="quiz-card">
                                    {/* Question Row */}
                                    <div className="question-row">
                                        <div className="input-wrapper">
                                            <label className="form-label">Question {qIndex + 1}</label>
                                            <input
                                                type="text"
                                                value={question.text}
                                                onChange={(e) => handleQuestionChange(qIndex, e)}
                                                className="form-input"
                                                placeholder="What is the question?"
                                                required
                                            />
                                        </div>
                                        <button
                                            type="button"
                                            className="btn btn-danger btn-icon delete-btn"
                                            onClick={() => handleDeleteQuestion(qIndex)}
                                            title="Delete Question"
                                        >
                                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
                                                <path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0V6z" />
                                                <path fillRule="evenodd" d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1v1zM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4H4.118zM2.5 3V2h11v1h-11z" />
                                            </svg>
                                        </button>
                                    </div>

                                    {/* Answers Section */}
                                    <div className="answers-section">
                                        <label className="form-label">Answers</label>
                                        <div className="answers-list">
                                            {question.answers.map((answer, aIndex) => (
                                                <div key={aIndex} className="answer-row">
                                                    <div className="radio-wrapper">
                                                        <input
                                                            type="radio"
                                                            name={`correct-answer-${qIndex}`}
                                                            checked={answer.isCorrect}
                                                            onChange={() => handleCorrectAnswerChange(qIndex, aIndex)}
                                                            className="answer-radio"
                                                            title="Mark as correct answer"
                                                        />
                                                    </div>
                                                    <div className="input-wrapper">
                                                        <input
                                                            type="text"
                                                            value={answer.text}
                                                            onChange={(e) => handleAnswerChange(qIndex, aIndex, e)}
                                                            className="form-input"
                                                            placeholder={`Option ${aIndex + 1}`}
                                                            required
                                                        />
                                                    </div>
                                                    <button
                                                        type="button"
                                                        className="btn btn-danger btn-icon delete-btn"
                                                        onClick={() => handleDeleteAnswer(qIndex, aIndex)}
                                                        title="Delete Answer"
                                                    >
                                                        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" viewBox="0 0 16 16">
                                                            <path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0V6z" />
                                                            <path fillRule="evenodd" d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1v1zM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4H4.118zM2.5 3V2h11v1h-11z" />
                                                        </svg>
                                                    </button>
                                                </div>
                                            ))}
                                        </div>
                                        <button
                                            type="button"
                                            onClick={() => handleAddAnswer(qIndex)}
                                            className="btn btn-secondary btn-sm add-answer-btn"
                                        >
                                            + Add Option
                                        </button>
                                    </div>
                                </div>
                            ))}
                        </div>

                        <div className="add-question-wrapper">
                            <button
                                type="button"
                                onClick={handleAddQuestion}
                                className="btn btn-secondary add-question-btn"
                            >
                                + Add New Question
                            </button>
                        </div>
                    </form>
                </div>

                <div className="modal-footer">
                    <button type="button" className="btn btn-secondary" onClick={onClose}>
                        Cancel
                    </button>
                    <button type="submit" form="quiz-form" className="btn btn-primary">
                        {quiz ? 'Save Quiz' : 'Create Quiz'}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default QuizModal;