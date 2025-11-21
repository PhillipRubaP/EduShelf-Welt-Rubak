import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Settings.css';

const Settings = () => {
    const [lightMode, setLightMode] = useState(false);
    const navigate = useNavigate();

    useEffect(() => {
        const currentTheme = localStorage.getItem('theme');
        if (currentTheme === 'light') {
            setLightMode(true);
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

    const handleClose = () => {
        navigate(-1);
    };

    return (
        <div className="settings-container">
            <div className="settings-box">
                <span className="close-button" onClick={handleClose}>&times;</span>
                <h2>Einstellungen</h2>
                <div className="form-group">
                    <label htmlFor="light-mode-switch">Light Mode</label>
                    <input
                        type="checkbox"
                        id="light-mode-switch"
                        checked={lightMode}
                        onChange={handleLightModeToggle}
                    />
                </div>
            </div>
        </div>
    );
};

export default Settings;