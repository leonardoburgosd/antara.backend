export const MainLayout = ({ sidebar, children }) => {
  return (
    <div className="flex h-screen bg-black text-white overflow-hidden">
      {sidebar && <aside className="shrink-0">{sidebar}</aside>}
      <main className="flex-1 overflow-y-auto">{children}</main>
    </div>
  )
}

export default MainLayout

