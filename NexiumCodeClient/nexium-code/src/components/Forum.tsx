import React, { useState, useMemo } from "react";
import "./Forum.css";
import { Link, useNavigate } from "react-router-dom";
import iconChat from "/icon1.png";

interface Topic {
  id: number;
  title: string;
  author: string;
  avatar: string;
  date: string;
  replies: number;
  lastReply: {
    author: string;
    time: string;
  };
  category: string;
  status: "open" | "solved";
}

export const Forum: React.FC = () => {
  const navigate = useNavigate();

  const [topics] = useState<Topic[]>([
    {
      id: 1,
      title: "Как оптимизировать React-приложение?",
      author: "Alina Rubak",
      avatar: "/avatar1.jpg",
      date: "2 дня назад",
      replies: 5,
      lastReply: { author: "Oleg Makey", time: "3 часа назад" },
      category: "JavaScript",
      status: "open",
    },
    {
      id: 2,
      title: "Ошибка при работе с асинхронностью в C#",
      author: "Ivan Petrov",
      avatar: "/avatar2.png",
      date: "5 дней назад",
      replies: 2,
      lastReply: { author: "Dmitry B.", time: "вчера" },
      category: "C#",
      status: "solved",
    },
    {
      id: 3,
      title: "Советы по верстке адаптивных сайтов?",
      author: "Maria Sidorova",
      avatar: "/avatar3.png",
      date: "неделю назад",
      replies: 7,
      lastReply: { author: "Alex K.", time: "5 часов назад" },
      category: "HTML/CSS",
      status: "open",
    },
  ]);

  const [categoryFilter, setCategoryFilter] = useState("Все");
  const [statusFilter, setStatusFilter] = useState("Все");
  const [sortBy, setSortBy] = useState("Популярные");
  const [searchQuery, setSearchQuery] = useState("");

  const filteredTopics = useMemo(() => {
    let filtered = [...topics];


    if (categoryFilter !== "Все") {
      filtered = filtered.filter((t) => t.category === categoryFilter);
    }


    if (statusFilter !== "Все") {
      filtered = filtered.filter((t) => t.status === statusFilter);
    }


    if (searchQuery.trim() !== "") {
      const query = searchQuery.toLowerCase();
      filtered = filtered.filter(
        (t) =>
          t.title.toLowerCase().includes(query) ||
          t.author.toLowerCase().includes(query) ||
          t.category.toLowerCase().includes(query)
      );
    }

    if (sortBy === "Популярные") filtered.sort((a, b) => b.replies - a.replies);
    if (sortBy === "Новые") filtered.sort((a, b) => b.id - a.id);

    return filtered;
  }, [topics, categoryFilter, statusFilter, sortBy, searchQuery]);

  return (
    <div className="forum-page">
      <div className="forum-header">
        <h1 className="forum-title">Форум</h1>
        <button
          className="create-topic-button"
          onClick={() => navigate("/forum/create")}
        >
          + Создать тему
        </button>
      </div>

  
      <div className="filters">
        <input
          type="text"
          placeholder="Поиск по содержимому..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="search-input"
        />

        <select className="filterBox" onChange={(e) => setCategoryFilter(e.target.value)}>
          <option>Все</option>
          <option>JavaScript</option>
          <option>C#</option>
          <option>HTML/CSS</option>
        </select>

        <select className="filterBox" onChange={(e) => setStatusFilter(e.target.value)}>
          <option>Все</option>
          <option value="open">Открыта</option>
          <option value="solved">Решена</option>
        </select>

        <select  className="filterBox" onChange={(e) => setSortBy(e.target.value)}>
          <option>Популярные</option>
          <option>Новые</option>
        </select>
      </div>


      <div className="topics-list">
        {filteredTopics.length === 0 ? (
          <div className="empty-message">Ничего не найдено по запросу 😕</div>
        ) : (
          filteredTopics.map((topic) => (
            <Link
              key={topic.id}
              to={`/forum/${topic.id}`}
              state={{ topic }}
              className="topic-card-link"
              style={{ textDecoration: "none", color: "inherit" }}
            >
              <div className="topic-card" style={{ cursor: "pointer" }}>
                <img
                  src={topic.avatar}
                  alt={topic.author}
                  className="topic-avatar"
                />
                <div className="topic-content">
                  <div className="topic-main">
                    <h3 className="topic-title">{topic.title}</h3>
                    <span className="topic-meta">
                      Автор: {topic.author} • {topic.date}
                    </span>
                  </div>
                  <div className="topic-info">
                    <span className="topic-replies">
                      <img className = "icon"src = {iconChat}></img> {topic.replies} ответов
                    </span>
                    <span className="topic-last">
                      Последний ответ: <b>{topic.lastReply.author}</b> —{" "}
                      {topic.lastReply.time}
                    </span>
                    <span
                      className={`topic-category ${topic.category.toLowerCase()}`}
                    >
                      {topic.category}
                    </span>
                    <span className={`topic-status ${topic.status}`}>
                      {topic.status === "solved" ? "Решено" : "Открыта"}
                    </span>
                  </div>
                </div>
              </div>
            </Link>
          ))
        )}
      </div>
    </div>
  );
};
