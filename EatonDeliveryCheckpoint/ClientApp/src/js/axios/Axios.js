import axios from 'axios';

const axiosRequest = axios.create({
    baseURL: "/api/delivery",
    headers: {
        'Content-Type': 'application/json'
    },
})

export const axiosDeliveryUploadPostApi = (data) => axiosRequest.post("/upload", data);
export const axiosDeliveryCargoGetApi = () => axiosRequest.get("/dnlist");
export const axiosDeliveryCargoStartPostApi = (data) => axiosRequest.post("/start", data);
export const axiosDeliveryCargoFinishPostApi = (data) => axiosRequest.post("/finish", data);