import React from 'react'
import Content from './Content'

const Layout = (props) => {
    const children = props.children

    return <Content children={children} />
}

export default Layout