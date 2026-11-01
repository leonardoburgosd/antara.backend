export const AuthLayout = ({ children }) => {
  return (
    <div className="min-h-screen bg-black text-white flex flex-col items-center justify-center">
      {children}
    </div>
  )
}

export default AuthLayout

