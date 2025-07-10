import React, { useState } from 'react';
import UploadDialog from './UploadDialog';

const Files = () => {
  const [isUploadDialogOpen, setUploadDialogOpen] = useState(false);

  return (
    <div>
      <h2>Dateien</h2>
      <p>Hier werden die hochgeladenen Dateien angezeigt.</p>
      <button onClick={() => setUploadDialogOpen(true)}>Datei hochladen</button>
      {isUploadDialogOpen && <UploadDialog onClose={() => setUploadDialogOpen(false)} />}
    </div>
  );
};

export default Files;