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
            <div className="modal-content">
                <div className="modal-close-button" onClick={onClose}>
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                </div>
                <h2>{quiz ? 'Edit Quiz' : 'Create Quiz'}</h2>
                <form onSubmit={handleSubmit}>
                    <div className="form-group title-form-group">
                        <label>Title</label>
                        <input type="text" value={title} onChange={(e) => setTitle(e.target.value)} required />
                    </div>
                    {questions.map((question, qIndex) => (
                        <div key={qIndex} className="question-block">
                            <div className="form-group question-header">
                                <label>Question {qIndex + 1}</label>
                                <input type="text" value={question.text} onChange={(e) => handleQuestionChange(qIndex, e)} required />
                                <span className="delete-quiz-btn" onClick={() => handleDeleteQuestion(qIndex)}>
                                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" className="bi bi-trash" viewBox="0 0 16 16">
                                        <path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0V6z" />
                                        <path fillRule="evenodd" d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1v1zM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4H4.118zM2.5 3V2h11v1h-11z" />
                                    </svg>
                                </span>
                            </div>
                            {question.answers.map((answer, aIndex) => (
                                <div key={aIndex} className="answer-block form-group">
                                    <input
                                        type="radio"
                                        name={`correct-answer-${qIndex}`}
                                        checked={answer.isCorrect}
                                        onChange={() => handleCorrectAnswerChange(qIndex, aIndex)}
                                    />
                                    <input
                                        type="text"
                                        value={answer.text}
                                        onChange={(e) => handleAnswerChange(qIndex, aIndex, e)}
                                        placeholder={`Answer ${aIndex + 1}`}
                                        required
                                    />
                                    <span className="delete-quiz-btn" onClick={() => handleDeleteAnswer(qIndex, aIndex)}>
                                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" className="bi bi-trash" viewBox="0 0 16 16">
                                            <path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0V6z" />
                                            <path fillRule="evenodd" d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1v1zM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4H4.118zM2.5 3V2h11v1h-11z" />
                                        </svg>
                                    </span>
                                </div>
                            ))}
                            <button type="button" onClick={() => handleAddAnswer(qIndex)} className="add-answer-btn">Add Answer</button>
                        </div>
                    ))}
                    <button type="button" onClick={handleAddQuestion} className="add-question-btn">Add Question</button>
                    <div className="modal-actions">
                        <button type="submit">{quiz ? 'Save' : 'Create'}</button>
                        <button type="button" onClick={onClose}>Cancel</button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default QuizModal;