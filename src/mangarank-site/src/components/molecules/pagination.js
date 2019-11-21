import React from 'react'
import { Link } from 'gatsby'
import {
  Pagination,
  PaginationItem,
  PaginationLink
} from 'reactstrap'

const _Pagination = ({ page, numPages, basePath, className }) => {
  const hasPrev = 1 < page
  const hasNext = page < numPages
  const toPrev = page <= 2 ? basePath : `${basePath}/${page - 1}`
  const toNext = `${basePath}/${page + 1}`
  return (
    <Pagination aria-label="Page navigation"
                listClassName={className}>
      {hasPrev && <PaginationItem>
        <PaginationLink previous tag={Link} to={toPrev}>
          前のページ
        </PaginationLink>
      </PaginationItem>}
      {hasNext && <PaginationItem>
        <PaginationLink next tag={Link} to={toNext}>
          次のページ
        </PaginationLink>
      </PaginationItem>}
    </Pagination>
  )
}

export default _Pagination
