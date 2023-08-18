import axios from 'axios';

const axiosRequest = axios.create({
    baseURL: "/api/delivery",
    headers: {
        'Content-Type': 'application/json'
    },
})

export const axiosDeliveryUploadPostApi = (data) => axiosRequest.post("/upload", data);
export const axiosDeliveryGetApi = () => axiosRequest.get("/dnlist");
export const axiosDeliveryStartPostApi = (data) => axiosRequest.post("/start", data);
export const axiosDeliveryFinishPostApi = (data) => axiosRequest.post("/finish", data);
export const axiosDeliveryDismissAlertPostApi = (data) => axiosRequest.post("/dismissalert", data);
export const axiosDeliveryQuitPostApi = (data) => axiosRequest.post("/quit", data);