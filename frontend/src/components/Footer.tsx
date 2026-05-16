import { Link } from "@tanstack/react-router";

export function Footer() {
  return (
    <footer className="border-t mt-auto py-6 text-sm text-muted-foreground">
      <div className="container mx-auto px-4 flex flex-col sm:flex-row items-center justify-between gap-2">
        <p>© {new Date().getFullYear()} PSI Trivia — VU student project</p>
        <Link to="/privacy" className="hover:text-foreground transition-colors">
          Privacy Policy
        </Link>
      </div>
    </footer>
  );
}