import React, { useEffect, useState } from 'react';
import { Stack, Button } from '@mui/material';
import HomeIcon from '@mui/icons-material/Home';
import { NAV_HOME, NAV_SEARCH, NAV_TITLE, NAV_UPLOAD } from '../constants';
import DeliveryDashboard from './DeliveryDashboard';
import UploadDashboard from './UploadDashboard';
import SearchDashboard from './SearchDashboard';
import Logo from '../../img/eaton_logo.jpg';
import { axiosDeliveryGetApi } from '../axios/Axios';

const Content = () => {
    const activeBtnClass = "btn btn-app p-0 h-100 btn-app-active"
    const inactiveBtnClass = "btn btn-app p-0 h-100"
    const [homeBtnClass, setHomeBtnClass] = useState(activeBtnClass)
    const [uploadBtnClass, setUploadBtnClass] = useState(inactiveBtnClass)
    const [searchBtnClass, setSearchBtnClass] = useState(inactiveBtnClass)
    const [contentPage, setContentPage] = useState(0) // 0: home, 1: upload, 2: search
    const [deliveryState, setDeliveryState] = useState(0) // 0: none, 1: deliverying
    const [deliveryCargoDtos, setDeliveryCargoDtos] = useState(null)

    useEffect(() => {
        const interval = setInterval(() => {
            requestGetApi()
        }, 5000)
        return () => clearInterval(interval)
    }, [deliveryState])

    const handleHomeClick = (e) => {
        setHomeBtnClass(activeBtnClass)
        setUploadBtnClass(inactiveBtnClass)
        setSearchBtnClass(inactiveBtnClass)
        setContentPage(0)
    }

    const handleUploadClick = (e) => {
        setHomeBtnClass(inactiveBtnClass)
        setUploadBtnClass(activeBtnClass)
        setSearchBtnClass(inactiveBtnClass)
        setContentPage(1)
    }

    const handleSearchClick = (e) => {
        setHomeBtnClass(inactiveBtnClass)
        setUploadBtnClass(inactiveBtnClass)
        setSearchBtnClass(activeBtnClass)
        setContentPage(2)
    }

    const renderContent = (contentState) => {
        switch (contentState) {
            case 0:
                return <DeliveryDashboard
                    setDeliveryState={setDeliveryState}
                    deliveryCargoDtos={deliveryCargoDtos}
                    requestGetApi={requestGetApi}/>
            case 1:
                return <UploadDashboard deliveryCargoDtos={deliveryCargoDtos} />
            case 2:
                return <SearchDashboard
                    deliveryCargoDtos={deliveryCargoDtos}/>
            default:
                return <></>
        }
    }

    async function requestGetApi() {
        try {
            const response = await axiosDeliveryGetApi()
            if (response.data.result === true) {
                setDeliveryCargoDtos(response.data.deliveryCargoDtos)
            }
        } catch (error) {
            console.log("requestGetApi error")
            console.log(error)
        }
    }

    return <div className="bg-eaton-b">
        <section className="content-header content-header-p-2 vh-10">
            <div className="row h-100">
                <div className="col-sm-4 h-100">
                    <img src={Logo} className="h-100" alt="logo" />
                </div>
                <div className="col-sm-4 h-100">
                    <div className="text-title d-none d-sm-block h-100">{NAV_TITLE}</div>
                </div>
                <div className="col-sm-4 h-100">
                    <nav className="navbar navbar-expand h-100 p-0">
                        <ul className="navbar-nav ml-auto h-100">
                            <li className="nav-item nav-link h-100">
                                <button type="button" className={homeBtnClass} onClick={handleHomeClick} >
                                    <i className="fas fa-home"></i>
                                    <label className="navbar-item-text">{NAV_HOME}</label>
                                </button>
                            </li>
                            <li className="nav-item nav-link h-100">
                                <button type="button" className={uploadBtnClass} onClick={handleUploadClick} disabled={deliveryState === 1} >
                                    <i className="fas fa-upload"></i>
                                    <label className="navbar-item-text">{NAV_UPLOAD}</label>
                                </button>
                            </li>
                            <li className="nav-item nav-link h-100">
                                <button type="button" className={searchBtnClass} onClick={handleSearchClick} disabled={deliveryState === 1}  >
                                    <i className="fas fa-search"></i>
                                    <label className="navbar-item-text">{NAV_SEARCH}</label>
                                </button>
                            </li>
                        </ul>
                    </nav>
                </div>
            </div>
        </section>
        <section className="content vh-90">
            <div className="container-fluid h-100 p-3">
                {renderContent(contentPage)}
            </div>
        </section>
    </div>
}

export default Content