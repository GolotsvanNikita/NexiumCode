import React, { useState } from "react";
import "./ForumThread.css";
import { useNavigate, useLocation, useParams } from "react-router-dom";
import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";
import { vscDarkPlus } from "react-syntax-highlighter/dist/esm/styles/prism";

interface Comment {
  id: number;
  author: string;
  avatar: string;
  text: string;
  date: string;
  parentId: number | null;
}

interface Topic {
  id: number;
  title: string;
  category: string;
  status: "open" | "solved";
}


const parseCommentText = (text: string) => {
  const parts = text.split(/```/);
  return parts.map((part, index) => {
    if (index % 2 === 1) {
      const lines = part.trim().split("\n");
      let language = "javascript";
      if (lines[0].match(/^[a-z]+$/i)) {
        language = lines[0];
        lines.shift();
      }
      return (
        <SyntaxHighlighter
          key={index}
          language={language}
          style={vscDarkPlus}
          customStyle={{
            borderRadius: "8px",
            fontSize: "14px",
            background: "#1e1e1e",
          }}
        >
          {lines.join("\n")}
        </SyntaxHighlighter>
      );
    }
    return <span key={index}>{part}</span>;
  });
};
export const ForumThread: React.FC = () => {
  const navigate = useNavigate();
  const { state } = useLocation();
  const { id } = useParams();
  const topic = (state as { topic?: Topic })?.topic;

  const [comments, setComments] = useState<Comment[]>([
    {
      id: 1,
      author: "Alina Rubak",
      avatar: "/avatar1.jpg",
      text: "Согласна, интересная тема 😎",
      date: "3 недели назад",
      parentId: null,
    },
  ]);

  const [newComment, setNewComment] = useState("");
  const addComment = () => {
    if (!newComment.trim()) return;
    setComments([
      ...comments,
      {
        id: Date.now(),
        author: "Вы",
        avatar: "/avatar3.png",
        text: newComment,
        date: Date(),
        parentId: null,
      },
    ]);
    setNewComment("");
  };

  const addReply = (parentId: number, text: string) => {
    if (!text.trim()) return;
    setComments([
      ...comments,
      {
        id: Date.now(),
        author: "Вы",
        avatar: "/avatar3.png",
        text,
        date: Date(),
        parentId,
      },
    ]);
  };

  const renderComments = (parentId: number | null = null) =>
    comments
      .filter((c) => c.parentId === parentId)
      .map((comment) => (
        <CommentItem
          key={comment.id}
          comment={comment}
          onReply={addReply}
          replies={renderComments(comment.id)}
        />
      ));

  return (
    <div className="thread-container">
      <button className="back-button" onClick={() => navigate("/forum")}>
        ← Назад к списку тем
      </button>

      {topic ? (
        <>
          <div className="topic-header">
            <div>
              <h2 className="topic-title">{topic.title}</h2>
              <span className="topic-category">Категория: {topic.category}</span>
            </div>
            <div
              className={`topic-status ${
                topic.status === "solved" ? "solved" : "open"
              }`}
            >
              {topic.status === "solved" ? "Решена" : "Открыта"}
            </div>
          </div>

          <div className="comments-list">{renderComments()}</div>

          <div className="new-comment">
            <img src="/avatar3.png" alt="Ваш аватар" className="avatar" />
            <div className="input-wrapper">
              <textarea
                placeholder='Напишите комментарий... (для кода используйте ``` )'
                value={newComment}
                onChange={(e) => setNewComment(e.target.value)}
                className="comment-textarea"
              ></textarea>
              <div className="comment-actions">
                <button onClick={addComment} className="comment-button">
                  Отправить
                </button>
              </div>
            </div>
          </div>
        </>
      ) : (
        <div className="topic-header">
          <h2>Тема #{id}</h2>
          <p>Данные темы не переданы (обновите страницу позже)</p>
        </div>
      )}
    </div>
  );
};

const CommentItem: React.FC<{
  comment: Comment;
  onReply: (parentId: number, text: string) => void;
  replies: React.ReactNode;
}> = ({ comment, onReply, replies }) => {
  const [isReplying, setIsReplying] = useState(false);
  const [replyText, setReplyText] = useState("");

  const handleReply = () => {
    if (!replyText.trim()) return;
    onReply(comment.id, replyText);
    setReplyText("");
    setIsReplying(false);
  };

  return (
    <div className="comment">
      <div className="comment-card">
        <div className="comment-header">
          <img src={comment.avatar} alt={comment.author} className="comment-avatar" />
          <div className="comment-info">
            <span className="comment-author">{comment.author}</span>
            <span className="comment-date">{comment.date}</span>
          </div>
        </div>
        <div className="comment-text">{parseCommentText(comment.text)}</div>
        <button className="reply-button" onClick={() => setIsReplying(!isReplying)}>
          Ответить
        </button>

        {isReplying && (
          <div className="reply-box">
            <textarea
              className="comment-textarea"
              placeholder="Напишите ответ..."
              value={replyText}
              onChange={(e) => setReplyText(e.target.value)}
            />
            <div className="reply-actions">
              <button onClick={handleReply} className="comment-button">
                Отправить
              </button>
              <button
                onClick={() => setIsReplying(false)}
                className="cancel-button"
              >
                Отмена
              </button>
            </div>
          </div>
        )}
      </div>

      {replies && <div className="replies">{replies}</div>}
    </div>
  );
};
