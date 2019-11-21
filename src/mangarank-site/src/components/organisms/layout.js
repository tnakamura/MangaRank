import React from 'react'
import PropTypes from 'prop-types'
import Helmet from 'react-helmet'
import { Container } from 'reactstrap'
import { StaticQuery, graphql } from 'gatsby'

import '../../styles/main.scss'
import Header from './header'
import Footer from './footer'

const Layout = ({ children, title }) => (
  <StaticQuery
    query={graphql`
      query SiteTitleQuery {
        site {
          siteMetadata {
            title
          }
        }
      }
    `}
    render={data => (
      <>
        <Helmet
          title={title
            ? `${title} - ${data.site.siteMetadata.title}`
            : data.site.siteMetadata.title}
          meta={[
            { name: 'description', content: '面白いマンガに出会うためのサービスです。' },
            { name: 'keywords', content: 'manga, comic' },
          ]}
        >
          <html lang="ja" />
        </Helmet>
        <Header siteTitle={data.site.siteMetadata.title} />
        <Container className="mt-4 pb-3">
          {children}
        </Container>
        <Footer />
      </>
    )}
  />
)

Layout.propTypes = {
  children: PropTypes.node.isRequired,
}

export default Layout
