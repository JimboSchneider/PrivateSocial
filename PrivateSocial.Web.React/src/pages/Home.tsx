function Home() {
  return (
    <div className="max-w-4xl mx-auto">
      <h1 className="text-2xl md:text-4xl lg:text-5xl font-bold text-gray-900 mb-4 md:mb-6">Welcome to PrivateSocial</h1>
      <p className="text-base md:text-lg lg:text-xl text-gray-600 mb-6 md:mb-8">Connect, share, and engage with your community.</p>
      
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 md:gap-6">
        <div className="card">
          <h3 className="text-lg md:text-xl font-semibold text-gray-800 mb-2">Share Your Thoughts</h3>
          <p className="text-sm md:text-base text-gray-600">Create posts and share what's on your mind with the community.</p>
        </div>
        <div className="card">
          <h3 className="text-lg md:text-xl font-semibold text-gray-800 mb-2">Connect with Others</h3>
          <p className="text-sm md:text-base text-gray-600">Engage with posts from other users and build meaningful connections.</p>
        </div>
        <div className="card">
          <h3 className="text-lg md:text-xl font-semibold text-gray-800 mb-2">Stay Updated</h3>
          <p className="text-sm md:text-base text-gray-600">Keep up with the latest posts and activities in your network.</p>
        </div>
      </div>
    </div>
  )
}

export default Home