import { useEffect, useLayoutEffect, useRef, useState } from "react";

/**
 * Racuna koliko visine element sme da zauzme da bi stao do dna prozora,
 * pa tabela skroluje unutar sebe, a zaglavlje kolona ostaje vidljivo.
 * Meri posle svakog prikaza, jer se iznad tabele mogu pojaviti filteri ili poruke.
 */
export default function useFitHeight({ reserve = 88, min = 280 } = {}) {
  const ref = useRef(null);
  const [height, setHeight] = useState(null);

  const measure = () => {
    const element = ref.current;
    if (!element) return;

    const top = element.getBoundingClientRect().top + window.scrollY;
    const next = Math.max(min, Math.floor(window.innerHeight - top - reserve));
    setHeight((current) => (current === next ? current : next));
  };

  useLayoutEffect(measure);

  useEffect(() => {
    window.addEventListener("resize", measure);
    return () => window.removeEventListener("resize", measure);
  });

  return [ref, height];
}
