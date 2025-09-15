import * as lucideIcons from "lucide-react";

export const { icons } = lucideIcons;

interface LucideProps extends React.ComponentPropsWithoutRef<"svg"> {
  icon: keyof typeof icons;
  title?: string;
}

function Lucide(props: LucideProps) {
  const { icon, className, ...computedProps } = props;
  const Component = icons[props.icon];
  return (
    <Component
      {...computedProps}
      className={props.className}
    />
  );
}

export default Lucide;
