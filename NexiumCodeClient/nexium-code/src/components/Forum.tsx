import React, { useState, useEffect, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { forumService, type Thread } from "../services/forumService";
import "./Forum.css";

export const Forum: React.FC = () => {
  const navigate = useNavigate();
  const [threads, setThreads] = useState<Thread[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<string>("all");
  const [searchQuery, setSearchQuery] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(5);
  const [hasMore, setHasMore] = useState(true);
  const categories = ["all", "JavaScript", "C#", "HTML/CSS", "Python", "Others"];

  const loadThreads = useCallback(async (append = false) => {
    try {
      setLoading(true);
      setError(null);

      const category = selectedCategory === "all" ? undefined : selectedCategory;
      const data = await forumService.getThreads(category, searchQuery || undefined, page, pageSize);

      if (append) {
        setThreads(prev => {
          const newItems = data.items.filter(newItem => !prev.some(existingItem => existingItem.id === newItem.id));
          return [...prev, ...newItems];
        });
      } else {
        setThreads(data.items);
      }

      const totalPages = Math.ceil(data.totalCount / pageSize);
      setHasMore(page < totalPages);
    } catch (err) {
      console.error("Error loading threads:", err);
      setError("Failed to load threads. Please try again.");
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, selectedCategory, searchQuery]); 

  useEffect(() => {

    loadThreads(page > 1); 
  }, [page, loadThreads, selectedCategory, searchQuery]);

  const handleLoadMore = () => {
    setPage((prevPage) => prevPage + 1);
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 60) return `${diffMins} min ago`;
    if (diffHours < 24) return `${diffHours} hours ago`;
    if (diffDays < 7) return `${diffDays} days ago`;
    return date.toLocaleDateString("en-US");
  };

  const handleThreadClick = (thread: Thread) => {
    navigate(`/forum/${thread.id}`, { state: { thread } });
  };

  const handleSearch = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchQuery(e.target.value);
    setThreads([]); 
    setPage(1);     
  };

  const handleCategoryChange = (cat: string) => {
    setSelectedCategory(cat);
    setThreads([]); 
    setPage(1);     
  };

  const ThreadSkeleton = () => (
    <div className="thread-card skeleton-card">
      <div className="skeleton skeleton-avatar"></div>
      <div className="thread-main">
        <div className="thread-info">
          <div className="skeleton skeleton-title"></div>
          <div className="skeleton skeleton-meta"></div>
        </div>
        <div className="thread-stats">
          <div className="skeleton skeleton-stat"></div>
          <div className="skeleton skeleton-status"></div>
        </div>
      </div>
    </div>
  );

  const extractFirstImage = (markdown: string): string | null => {
    const match = markdown.match(/!\[.*?\]\((.*?)\)/);
    return match ? match[1] : null;
  };


  return (
    <div className="forum-container">
      <div className="forum-header">
        <h1>Community Forum</h1>
        <button className="create-thread-btn" onClick={() => navigate("/forum/create")}>
          + Create Thread
        </button>
      </div>

      <div className="forum-search">
        <input
          type="text"
          placeholder="Search threads..."
          value={searchQuery}
          onChange={handleSearch}
          className="search-input"
        />
      </div>

      <div className="forum-filters">
        {categories.map((cat) => (
          <button
            key={cat}
            className={`filter-btn ${selectedCategory === cat ? "active" : ""}`} 
            onClick={() => handleCategoryChange(cat)}
          >
            {cat === "all" ? "All" : cat}
          </button>
        ))}
      </div>

      {loading && (
        <div className="threads-list">
          {[1, 2, 3, 4, 5].map((n) => (
            <ThreadSkeleton key={n} />
          ))}
        </div>
      )}

      {error && <div className="error-message">{error}</div>}

      {!loading && !error && threads.length === 0 && (
        <div className="no-threads">
          <p>No threads found</p>
          <button onClick={() => navigate("/forum/create")}>Create the first thread</button>
        </div>
      )}

      {!loading && !error && (
        <div className="threads-list">
              {threads.map((thread) => {
                const imageUrl = extractFirstImage(thread.content);

                return (
                  <div
                    key={thread.id}
                    className="thread-card"
                    onClick={() => handleThreadClick(thread)}
                  >
                    <img
                      src={
                        thread.avatarUrl
                          ? `http://localhost:5064${thread.avatarUrl}`
                          : "http://localhost:5064/images/avatars/default-avatar.png"
                      }
                      alt={thread.username}
                      className="thread-avatar"
                      onClick={(e) => {
                        e.stopPropagation();
                        navigate(`/profile/${thread.userId}`);
                      }}
                    />

                    <div className="thread-main">
                      <div className="thread-info">
                        <h3 className="thread-title">{thread.title}</h3>
                        <div className="thread-meta">
                          <span
                            className="thread-author"
                            onClick={(e) => {
                              e.stopPropagation();
                              navigate(`/profile/${thread.userId}`);
                            }}
                          >
                            {thread.username}
                          </span>
                          <span className="thread-date">{formatDate(thread.createdAt)}</span>
                          <span
                            className={`thread-category ${thread.category
                              .toLowerCase()
                              .replace(/[\/]/g, "-")}`}
                          >
                            {thread.category}
                          </span>
                        </div>
                        {imageUrl && (
                          <img
                            src={imageUrl}
                            alt="Preview"
                            className="thread-preview-image"
                            style={{ maxWidth: "200px", borderRadius: "8px", marginTop: "8px" }}
                          />
                        )}
                      </div>
                      <div className="thread-stats">
                        <div className="stat">
                          <span className="stat-value">{thread.replyCount}</span>
                          <span className="stat-label">replies</span>
                        </div>
                        <div className={`thread-status ${thread.isResolved ? "resolved" : "open"}`}>
                          {thread.isResolved ? "✓ Solved" : "Open"}
                        </div>
                      </div>
                    </div>
                  </div>
                );
              })}

        </div>
      )}
      {!loading && hasMore && (
        <div className="load-more-container">
          <button className="load-more-btn" onClick={handleLoadMore}>
            Load more
          </button>
        </div>
      )}
    </div>
  );
};