import { useEffect, useRef } from 'react';
import { renderAsync } from 'docx-preview';

const DocxViewer = ({ blob }) => {
  const containerRef = useRef(null);

  useEffect(() => {
    if (blob && containerRef.current) {
      renderAsync(blob, containerRef.current);
    }
  }, [blob]);

  return <div ref={containerRef} className="docx-container" />;
};

export default DocxViewer;