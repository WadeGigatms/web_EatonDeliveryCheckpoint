import { makeStyles } from "@mui/styles";
import { alpha } from "@mui/material";

export const useStyles = makeStyles({
    "@keyframes blinker": {
        from: {
            backgroundColor: alpha("#76ff03", 1)
        },
        to: {
            backgroundColor: alpha("#76ff03", 0.3)
        }
    },
    "@keyframes invalidBlinker": {
        from: {
            backgroundColor: alpha("#ff1744", 1)
        },
        to: {
            backgroundColor: alpha("#ff1744", 0.3)
        }
    },
    blinker: {
        animationName: "$blinker",
        animationDuration: "500ms",
        animationIterationCount: "infinite",
        animationDirection: "alternate",
        animationTimingFunction: "ease-in-out",
        animationPlayState: "running"
    },
    invalidBlinker: {
        animationName: "$invalidBlinker",
        animationDuration: "500ms",
        animationIterationCount: "infinite",
        animationDirection: "alternate",
        animationTimingFunction: "ease-in-out",
        animationPlayState: "running"
    }
})