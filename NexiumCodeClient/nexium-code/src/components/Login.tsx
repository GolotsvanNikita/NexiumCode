export const Login: React.FC<{ login: () => void }> = ({ login }) =>
(
    <div>
        <h1>Login</h1>
        <button onClick={login}>Login</button>
    </div>
);