import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Settings.css';

const Settings = () => {
    const [lightMode, setLightMode] = useState(false);
    const [animationsEnabled, setAnimationsEnabled] = useState(true);
    const [compactMode, setCompactMode] = useState(false);
    const [soundEffects, setSoundEffects] = useState(true);
    const navigate = useNavigate();

    useEffect(() => {
        const currentTheme = localStorage.getItem('theme');
        if (currentTheme === 'light') {
            setLightMode(true);
        }

        const storedAnimations = localStorage.getItem('animationsEnabled');
        if (storedAnimations !== null) {
            setAnimationsEnabled(storedAnimations === 'true');
        }

        const storedCompact = localStorage.getItem('compactMode');
        if (storedCompact !== null) {
            setCompactMode(storedCompact === 'true');
        }

        const storedSound = localStorage.getItem('soundEffects');
        if (storedSound !== null) {
            setSoundEffects(storedSound === 'true');
        }
    }, []);

    const handleLightModeToggle = () => {
        const newLightMode = !lightMode;
        setLightMode(newLightMode);
        if (newLightMode) {
            document.documentElement.setAttribute('data-theme', 'light');
            localStorage.setItem('theme', 'light');
        } else {
            document.documentElement.removeAttribute('data-theme');
            localStorage.removeItem('theme');
        }
    };

    const handleAnimationsToggle = () => {
        const newValue = !animationsEnabled;
        setAnimationsEnabled(newValue);
        localStorage.setItem('animationsEnabled', String(newValue));
    };

    const handleCompactModeToggle = () => {
        const newValue = !compactMode;
        setCompactMode(newValue);
        localStorage.setItem('compactMode', String(newValue));
        if (newValue) {
            document.body.classList.add('compact-mode');
        } else {
            document.body.classList.remove('compact-mode');
        }
    };

    const handleSoundEffectsToggle = () => {
        const newValue = !soundEffects;
        setSoundEffects(newValue);
        localStorage.setItem('soundEffects', String(newValue));
    };


    const handleClose = () => {
        navigate(-1);
    };

    return (
        <div className="modal-overlay" onClick={handleClose}>
            <div className="modal-container settings-modal" onClick={(e) => e.stopPropagation()}>
                <button className="modal-close-button" onClick={handleClose} aria-label="Close">
                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <line x1="18" y1="6" x2="6" y2="18"></line>
                        <line x1="6" y1="6" x2="18" y2="18"></line>
                    </svg>
                </button>

                <div className="modal-header">
                    <h2 className="modal-title">Settings</h2>
                    <p className="settings-subtitle">Customize EduShelf to your needs</p>
                </div>

                <div className="modal-body settings-body">
                    <div className="settings-section">
                        <h3 className="section-title">Appearance</h3>

                        <div className="setting-item">
                            <div className="setting-info">
                                <span className="setting-label">Light Mode</span>
                                <span className="setting-desc">Light theme for better visibility</span>
                            </div>
                            <label className="toggle-switch">
                                <input
                                    type="checkbox"
                                    checked={lightMode}
                                    onChange={handleLightModeToggle}
                                />
                                <span className="slider round"></span>
                            </label>
                        </div>

                        <div className="setting-item disabled">
                            <div className="setting-info">
                                <span className="setting-label">Compact Mode <span className="soon-badge">Coming Soon</span></span>
                                <span className="setting-desc">More content in less space</span>
                            </div>
                            <label className="toggle-switch">
                                <input
                                    type="checkbox"
                                    checked={compactMode}
                                    onChange={handleCompactModeToggle}
                                    disabled
                                />
                                <span className="slider round"></span>
                            </label>
                        </div>
                    </div>

                    <div className="settings-section">
                        <h3 className="section-title">Experience</h3>

                        <div className="setting-item disabled">
                            <div className="setting-info">
                                <span className="setting-label">Animations <span className="soon-badge">Coming Soon</span></span>
                                <span className="setting-desc">Visual effects for smoother transitions</span>
                            </div>
                            <label className="toggle-switch">
                                <input
                                    type="checkbox"
                                    checked={animationsEnabled}
                                    onChange={handleAnimationsToggle}
                                    disabled
                                />
                                <span className="slider round"></span>
                            </label>
                        </div>

                        <div className="setting-item disabled">
                            <div className="setting-info">
                                <span className="setting-label">Sound Effects <span className="soon-badge">Coming Soon</span></span>
                                <span className="setting-desc">Play sounds on interactions</span>
                            </div>
                            <label className="toggle-switch">
                                <input
                                    type="checkbox"
                                    checked={soundEffects}
                                    onChange={handleSoundEffectsToggle}
                                    disabled
                                />
                                <span className="slider round"></span>
                            </label>
                        </div>
                    </div>
                </div>
                <div className="modal-footer">
                    <span className="version-info">Version 1.0.2</span>
                </div>
            </div>
        </div>
    );
};

export default Settings;