import React from 'react';

function Dashboard() {
  return (
    <div className="dashboard">
      <h2 className="text-2xl font-bold mb-4">Dashboard</h2>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div className="card">
          <h3 className="font-bold">Recent Files</h3>
          {/* Placeholder for recent files */}
        </div>
        <div className="card">
          <h3 className="font-bold">Favorite Files</h3>
          {/* Placeholder for favorite files */}
        </div>
        <div className="card">
          <h3 className="font-bold">Upload Statistics</h3>
          {/* Placeholder for upload statistics */}
        </div>
      </div>
    </div>
  );
}

export default Dashboard;