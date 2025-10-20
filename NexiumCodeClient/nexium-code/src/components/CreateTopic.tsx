import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import ReactMarkdown from "react-markdown";
import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";
import { materialOceanic } from "react-syntax-highlighter/dist/esm/styles/prism";
import "./CreateTopic.css";

export const CreateTopic: React.FC = () => {
  const navigate = useNavigate();
  const [title, setTitle] = useState("");
  const [category, setCategory] = useState("JavaScript");
  const [content, setContent] = useState("");
  const [preview, setPreview] = useState(false);
  const [status, setStatus] = useState<"open" | "solved">("open");

  const handlePublish = () => {
    if (!title.trim() || !content.trim()) {
      alert("Заполните все поля");
      return;
    }
    alert(`Тема опубликована! Статус: ${status} (пока только фронт, потом будет сервер)`);
    navigate("/forum");
  };

  const insertCodeBlock = () => {
    const textarea = document.getElementById("topic-textarea") as HTMLTextAreaElement;
    if (!textarea) return;

    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const before = content.substring(0, start);
    const after = content.substring(end);
    const codeTemplate = `\n\`\`\`js\n// ваш код здесь\n\`\`\`\n`;

    const newContent = before + codeTemplate + after;
    setContent(newContent);

    setTimeout(() => {
      textarea.focus();
      const cursorPosition = start + 8;
      textarea.selectionStart = textarea.selectionEnd = cursorPosition;
    }, 0);
  };

  return (
    <div className="create-topic-container">
      <div className="backButtonDiv">
        <button onClick={() => navigate("/forum")} className="back-button">
          ← Назад
        </button>
      </div>

      <h2>Создание новой темы</h2>

      <input
        type="text"
        placeholder="Заголовок темы"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        className="topic-input"
      />

      <select
        value={category}
        onChange={(e) => setCategory(e.target.value)}
        className="topic-select"
      >
        <option>JavaScript</option>
        <option>C#</option>
        <option>HTML/CSS</option>
        <option>Python</option>
      </select>

      {/* Новый селект для статуса */}
      <select
        value={status}
        onChange={(e) => setStatus(e.target.value as "open" | "solved")}
        className="topic-select"
      >
        <option value="open">Открыта</option>
        <option value="solved">Решена</option>
      </select>

      <div className="editor-toolbar">
        <button onClick={() => setContent((p) => p + "**жирный** ")}>B</button>
        <button onClick={() => setContent((p) => p + "_курсив_ ")}>I</button>
        <button onClick={insertCodeBlock}>{`</>`}</button>
        <button onClick={() => setPreview(!preview)}>👁 Предпросмотр</button>
      </div>

      {!preview ? (
        <textarea
          id="topic-textarea"
          value={content}
          onChange={(e) => setContent(e.target.value)}
          className="topic-textarea"
          placeholder="Введите текст темы..."
        />
      ) : (
        <div className="topic-preview">
          <ReactMarkdown
            children={content}
            components={{
              code({ className, children }) {
                const match = /language-(\w+)/.exec(className || "");
                return match ? (
                  <SyntaxHighlighter
                    style={materialOceanic}
                    language={match[1]}
                    PreTag="div"
                  >
                    {String(children).replace(/\n$/, "")}
                  </SyntaxHighlighter>
                ) : (
                  <code className={className}>{children}</code>
                );
              },
            }}
          />
        </div>
      )}

      <button onClick={handlePublish} className="publish-button">
        Опубликовать
      </button>
    </div>
  );
};
