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

    const handleAddAnswer = (questionIndex) => {
        const newQuestions = [...questions];
        newQuestions[questionIndex].answers.push({ text: '', isCorrect: false });
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
                <span className="close-button" onClick={onClose}>&times;</span>
                <h2>{quiz ? 'Edit Quiz' : 'Create Quiz'}</h2>
                <form onSubmit={handleSubmit}>
                    <div className="form-group">
                        <label>Title</label>
                        <input type="text" value={title} onChange={(e) => setTitle(e.target.value)} required />
                    </div>
                    {questions.map((question, qIndex) => (
                        <div key={qIndex} className="question-block">
                            <div className="form-group">
                                <label>Question {qIndex + 1}</label>
                                <input type="text" value={question.text} onChange={(e) => handleQuestionChange(qIndex, e)} required />
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