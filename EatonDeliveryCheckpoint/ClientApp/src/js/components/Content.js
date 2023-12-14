import React, { useEffect, useState, useCallback } from 'react';
import { NAV_HOME, NAV_SEARCH, NAV_TITLE, NAV_UPLOAD } from '../constants';
import DeliveryDashboard from './DeliveryDashboard';
import FileDashboard from './FileDashboard';
import SearchDashboard from './SearchDashboard';
import Logo from '../../img/eaton_logo.jpg';
import { axiosDeliveryGetApi } from '../axios/Axios';

const Content = () => {
    const activeBtnClass = "btn btn-app p-0 h-100 btn-app-active"
    const inactiveBtnClass = "btn btn-app p-0 h-100"
    const [homeBtnClass, setHomeBtnClass] = useState(activeBtnClass)
    const [uploadBtnClass, setUploadBtnClass] = useState(inactiveBtnClass)
    const [searchBtnClass, setSearchBtnClass] = useState(inactiveBtnClass)
    const [contentPage, setContentPage] = useState("home") // 0: home, 1: upload, 2: search
    const [buttonDisabled, setButtonDisabled] = useState(false)
    const [deliveryStep, setDeliveryStep] = useState("new")
    // 0: new, 1: select, 2: deliverying, 3: finish/search/review, 4: edit, -1: alert/pause
    const [deliveryNumberDtos, setDeliveryNumberDtos] = useState(null)

    useEffect(() => {
        const interval = setInterval(() => {
            requestGetApi()
        }, 5000)
        return () => clearInterval(interval)
    }, [])

    useEffect(() => {
        setButtonDisabled(deliveryStep === "delivery" || deliveryStep === "alert" ? true : false)
    }, [deliveryStep])

    async function requestGetApi() {
        try {
            const response = await axiosDeliveryGetApi();
            if (response.data.result === true) {
                setDeliveryNumberDtos(response.data.deliveryNumberDtos);
            }
        } catch (error) {
            console.log("requestGetApi error");
            console.log(error);
        }
    }

    const handleHomeClick = (e) => {
        setHomeBtnClass(activeBtnClass)
        setUploadBtnClass(inactiveBtnClass)
        setSearchBtnClass(inactiveBtnClass)
        setContentPage("home")
    }

    const handleUploadClick = (e) => {
        setHomeBtnClass(inactiveBtnClass)
        setUploadBtnClass(activeBtnClass)
        setSearchBtnClass(inactiveBtnClass)
        setContentPage("upload")
    }

    const handleSearchClick = (e) => {
        setHomeBtnClass(inactiveBtnClass)
        setUploadBtnClass(inactiveBtnClass)
        setSearchBtnClass(activeBtnClass)
        setContentPage("search")
    }

    const renderContent = (contentState) => {
        switch (contentState) {
            case "home":
                return <DeliveryDashboard
                    deliveryStep={deliveryStep}
                    setDeliveryStep={setDeliveryStep}
                    deliveryNumberDtos={deliveryNumberDtos}
                    setDeliveryNumberDtos={setDeliveryNumberDtos} />
            case "upload":
                return <FileDashboard deliveryNumberDtos={deliveryNumberDtos} />
            case "search":
                return <SearchDashboard deliveryNumberDtos={deliveryNumberDtos} />
            default:
                return <></>
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
                                <button type="button" className={uploadBtnClass} onClick={handleUploadClick} disabled={buttonDisabled} >
                                    <i className="fas fa-upload"></i>
                                    <label className="navbar-item-text">{NAV_UPLOAD}</label>
                                </button>
                            </li>
                            <li className="nav-item nav-link h-100">
                                <button type="button" className={searchBtnClass} onClick={handleSearchClick} disabled={buttonDisabled} >
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