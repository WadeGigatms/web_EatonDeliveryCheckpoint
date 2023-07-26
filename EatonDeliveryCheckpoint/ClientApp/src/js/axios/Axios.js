import axios from 'axios';

const axiosDeliveryRequest = axios.create({
    baseURL: "/api/delivery",
    headers: {
        'Content-Type': 'application/json'
    },
})

export const axiosDeliveryUpload = (data) => axiosDeliveryRequest.post("/upload", data);

export const axiosDeliveryCargo = () => axiosDeliveryRequest.get("/cargo");